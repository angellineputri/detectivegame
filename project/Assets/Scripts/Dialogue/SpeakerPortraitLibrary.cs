using UnityEngine;

[CreateAssetMenu(menuName = "CityOfLies/Speaker Portrait Library", fileName = "SpeakerPortraitLibrary")]
public class SpeakerPortraitLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string speakerName;
        public Sprite portrait;
    }

    public Entry[] entries;

    public Sprite GetPortrait(string speakerName)
    {
        if (string.IsNullOrEmpty(speakerName) || entries == null)
        {
            return null;
        }

        foreach (Entry entry in entries)
        {
            if (entry.speakerName != null &&
                entry.speakerName.Trim().Equals(speakerName.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return entry.portrait;
            }
        }

        return null;
    }
}