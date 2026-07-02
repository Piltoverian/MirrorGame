using UnityEngine;

public class PanelChangeButton : MonoBehaviour
{
    [SerializeField] private GameObject Panel;

    public void OnClick()
    {
        if (Panel != null)
        {
            transform.parent.gameObject.SetActive(false);
            Panel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Panel GameObject is not assigned.");
        }
    }
}
