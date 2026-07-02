using TMPro;
using UnityEngine;

public class DoorLuminosityDisplay : MonoBehaviour
{
    [SerializeField] private Color openedColor;
    [SerializeField] private Color closedColor;
    private void Start()
    {
        GetComponent<TextMeshPro>().text = GetComponentInParent<LightReceiver>().GetLumosityToOpen().ToString("F2");
    }
    private void Update()
    {
        if (GetComponentInParent<LightReceiver>().IsOpened())
        {
            GetComponent<TextMeshPro>().color = openedColor;
        }
        else
        {
            GetComponent<TextMeshPro>().color = closedColor;
        }
    }
}
