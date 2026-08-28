using UnityEngine;

public class PersistentSystemsBootstrapper : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
