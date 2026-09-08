using UnityEngine;

[CreateAssetMenu(menuName = "CityOfLies/Case Data")]
public class CaseData : ScriptableObject
{
    public string caseID;

    public string caseName;

    [TextArea(2, 4)]
    public string description;

    public string startingScene;

    public bool isUnlocked;
}
