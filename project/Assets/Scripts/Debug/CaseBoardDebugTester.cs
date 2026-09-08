using UnityEngine;

public class CaseBoardDebugTester : MonoBehaviour
{
    void OnGUI()
    {
        var bm = CaseBoardManager.Instance;
        if (bm == null) { GUI.Label(new Rect(10,10,220,20), "CaseBoardManager not found"); return; }

        if (GUI.Button(new Rect(10,  10, 220, 28), "ExGf  P1 Pin"))       bm.ExGf_P1_Pin();
        if (GUI.Button(new Rect(10,  44, 220, 28), "ExGf  P1 Arrest"))    bm.ExGf_P1_Arrest();
        if (GUI.Button(new Rect(10,  78, 220, 28), "ExGf  P2 Pin"))       bm.ExGf_P2_Pin();
        if (GUI.Button(new Rect(10, 112, 220, 28), "ExGf  P2 Arrest"))    bm.ExGf_P2_Arrest();
        if (GUI.Button(new Rect(10, 146, 220, 28), "ExGf  P2 CourtDone")) bm.ExGf_P2_CourtDone();

        if (GUI.Button(new Rect(10, 196, 220, 28), "Asst  P1 Pin"))       bm.Assistant_P1_Pin();
        if (GUI.Button(new Rect(10, 230, 220, 28), "Asst  P1 Arrest"))    bm.Assistant_P1_Arrest();
        if (GUI.Button(new Rect(10, 264, 220, 28), "Asst  P2 Pin"))       bm.Assistant_P2_Pin();
        if (GUI.Button(new Rect(10, 298, 220, 28), "Asst  P2 Arrest"))    bm.Assistant_P2_Arrest();
        if (GUI.Button(new Rect(10, 332, 220, 28), "Asst  P2 CourtDone")) bm.Assistant_P2_CourtDone();

        if (GUI.Button(new Rect(10, 382, 220, 28), "Mark P3 Done"))       bm.MarkP3Done();

        GUI.Label(new Rect(10, 430, 300, 20),
            $"stageA={bm.stageA}  stageB={bm.stageB}  stageFinal={bm.stageFinal}");
    }
}
