using UnityEngine;
using UnityEditor;

public static class AutoFillPortraitsFromLibrary
{
    [MenuItem("CityOfLies/Auto-Fill Portraits From Library")]
    static void AutoFill()
    {
        string[] libGuids = AssetDatabase.FindAssets("t:SpeakerPortraitLibrary");
        if (libGuids.Length == 0)
        {
            Debug.LogError("No SpeakerPortraitLibrary asset found in the project.");
            return;
        }

        string libPath = AssetDatabase.GUIDToAssetPath(libGuids[0]);
        SpeakerPortraitLibrary library = AssetDatabase.LoadAssetAtPath<SpeakerPortraitLibrary>(libPath);

        string[] guids = AssetDatabase.FindAssets("t:DialogueData");
        int assetsTouched = 0;
        int linesTouched = 0;
        int linesSkippedNoMatch = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueData data = AssetDatabase.LoadAssetAtPath<DialogueData>(path);

            if (data == null || data.lines == null)
            {
                continue;
            }

            bool changed = false;

            foreach (DialogueLine line in data.lines)
            {
                if (line.speakerPortrait != null)
                {
                    continue; // don't overwrite anything already assigned
                }

                Sprite match = library.GetPortrait(line.speaker);
                if (match != null)
                {
                    line.speakerPortrait = match;
                    changed = true;
                    linesTouched++;
                }
                else
                {
                    linesSkippedNoMatch++;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(data);
                assetsTouched++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Auto-fill complete: {assetsTouched} assets updated, {linesTouched} lines filled, {linesSkippedNoMatch} lines had no library match.");
    }
}