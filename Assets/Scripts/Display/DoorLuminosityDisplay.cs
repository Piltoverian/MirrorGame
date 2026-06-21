using TMPro;
using UnityEngine;

public class DoorLuminosityDisplay : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TextMeshPro>().text = GetComponentInParent<LightReceiver>().GetLumosityToOpen().ToString("F2");
    }
}
