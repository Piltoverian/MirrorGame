using UnityEngine;
using UnityEngine.SceneManagement;
public class ChangeSceneButton : MonoBehaviour
{
    [SerializeField] protected string sceneName;
    public void OnClick()
    {
        SceneManager.LoadScene(sceneName);
    }    
}
