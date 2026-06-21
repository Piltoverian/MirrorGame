using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneManager : MonoBehaviour
{
    private SceneManager __instance;

    public SceneManager Instance
    {
        get
        {
            if (__instance == null)
            {
                __instance = FindAnyObjectByType<SceneManager>();
                if (__instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    __instance = singletonObject.AddComponent<SceneManager>();
                    singletonObject.name = typeof(SceneManager).ToString();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return __instance;
        }
    }

    public void Awake()
    {
        var sceneManagers = FindObjectsByType<SceneManager>(FindObjectsSortMode.None);
        for (int i = 1; i < sceneManagers.Length; i++)
        {
            Destroy(sceneManagers[i].gameObject);
        }

        DontDestroyOnLoad(sceneManagers[0].gameObject);
        __instance= sceneManagers[0];
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
