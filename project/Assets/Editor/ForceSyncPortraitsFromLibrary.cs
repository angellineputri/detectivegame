using UnityEngine;
using UnityEditor;

public static class ForceSyncPortraitsFromLibrary
{
    [MenuItem("CityOfLies/Force-Sync Portraits From Library")]
    static void ForceSync()
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
        int linesChanged = 0;
        int linesUnmatched = 0;

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
                Sprite libraryMatch = library.GetPortrait(line.speaker);

                if (libraryMatch == null)
                {
                    linesUnmatched++;
                    continue; // no entry for this speaker, leave line as-is
                }

                if (line.speakerPortrait != libraryMatch)
                {
                    line.speakerPortrait = libraryMatch;
                    changed = true;
                    linesChanged++;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(data);
                assetsTouched++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Force-sync complete: {assetsTouched} assets updated, {linesChanged} lines changed, {linesUnmatched} lines had no library entry (left untouched).");
    }
}