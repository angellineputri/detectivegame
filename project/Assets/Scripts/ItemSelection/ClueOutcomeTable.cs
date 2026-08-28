using UnityEngine;

[CreateAssetMenu(menuName = "CityOfLies/Clue Outcome Table", fileName = "NewOutcomeTable")]
public class ClueOutcomeTable : ScriptableObject
{
    public ClueOutcome[] outcomes;

    public ClueOutcome GetOutcome(string clueID)
    {
        foreach (ClueOutcome o in outcomes)
        {
            if (o.clueID == clueID)
            {
                return o;
            }
        }
        return null;
    }
}
