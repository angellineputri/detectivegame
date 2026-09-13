using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSystemsBootstrapper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        GameObject prefab = Resources.Load<GameObject>("PersistentSystems");

        if (prefab == null)
        {
            return;
        }

        GameObject instance = Object.Instantiate(prefab);
        Object.DontDestroyOnLoad(instance);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isMenuScene = scene.name == "MainMenu" || scene.name == "SelectCaseScreen" || scene.name == "SampleScene"
                        || scene.name == "VictimApartment"
                        || scene.name == "ExGF_PhoneCallScene_P2"
                        || scene.name == "ExGF_Court_P2";

if (BagUI.Instance != null)
        {
            BagUI.Instance.SetHudVisible(!isMenuScene);
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
