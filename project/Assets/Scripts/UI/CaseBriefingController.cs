using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CaseBriefingController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] TMP_Text infoText;
    [SerializeField] TMP_Text objectivesText;

    [Header("Evidence photo — assign sprite in Inspector")]
    [Tooltip("Assign your evidence photo sprite here. The Image is already framed with a mat and tilted backing card.")]
    [SerializeField] Image evidencePhoto;

    [Header("Buttons")]
    [SerializeField] Button backButton;
    [SerializeField] Button confirmButton;

    [Header("Scene names")]
    [SerializeField] string backScene    = "MainMenu";
    [SerializeField] string confirmScene = "VictimApartment";

    [Header("Copy (editable in Inspector)")]
    [SerializeField, TextArea(3, 8)]
    string briefingBody =
        "Julian Reyes is dead, and everyone close to him has something to hide.\n" +
        "Investigator: You.\n" +
        "Guided by Chief Inspector Marcus Doyle, uncover the truth behind the murder. " +
        "One suspect, one lie at a time.";

    [SerializeField, TextArea(3, 8)]
    string objectivesBody =
        "Julian Reyes is dead, and everyone close to him has something to hide.\n" +
        "Investigator: You.\n" +
        "Guided by Chief Inspector Marcus Doyle, uncover the truth behind the murder. " +
        "One suspect, one lie at a time.";

    void Start()
    {
        if (infoText      != null) infoText.text      = briefingBody;
        if (objectivesText != null) objectivesText.text = objectivesBody;

        backButton?.onClick.AddListener(OnBack);
        confirmButton?.onClick.AddListener(OnConfirm);
    }

    public void OnBack() => Load(backScene);

    public void OnComingSoon() => Debug.Log("[CaseBriefing] Coming soon!");

    public void OnConfirm()
    {
        GameManager.Instance?.SelectCase("city_of_lies");
        Load(confirmScene);
    }

    void Load(string scene)
    {
        if (string.IsNullOrEmpty(scene)) return;
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene(scene);
        else
            SceneManager.LoadScene(scene);
    }
}
