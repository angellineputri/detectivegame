using UnityEngine;

[CreateAssetMenu(menuName = "CityOfLies/Case Data")]
public class CaseData : ScriptableObject
{
    public string caseID;

    public string caseName;

    [TextArea(2, 4)]
    public string description;

    // Scene to load when this case begins (e.g. "Diner" for the ex-GF path).
    // GUESS: AssistantCase.startingScene will need updating once those scenes exist.
    public string startingScene;

    public bool isUnlocked;
}
