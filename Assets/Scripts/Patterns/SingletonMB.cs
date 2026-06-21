using UnityEngine;

public class SingletonMB<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool _isShuttingDown = false;
    private static object _lock = new object();

    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_isShuttingDown)
            {
                Debug.Log("[Singleton] Instance '" + typeof(T)
                    + "' already destroyed.");
                return null;
            }
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindFirstObjectByType(typeof(T));
                    // if cant find any object, create new object with T
                    if (_instance == null)
                    {
                        GameObject singletonObj = new GameObject();
                        _instance = singletonObj.AddComponent<T>();
                        singletonObj.name = typeof(T).ToString() + " (Singleton)";

                        Debug.Log("[Singleton] Create new object of Instance '"
                            + typeof(T).ToString() + "'.");

                        DontDestroyOnLoad(singletonObj);
                    }
                }
                return _instance;
            }
        }
    }
    private void OnApplicationQuit()
    {
        _isShuttingDown = true;
    }
    private void OnDestroy()
    {
        _isShuttingDown = true;
    }
}