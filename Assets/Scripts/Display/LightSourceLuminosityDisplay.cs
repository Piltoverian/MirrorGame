using TMPro;
using UnityEngine;

public class LightSourceLuminosityDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        GetComponent<TextMeshPro>().text = GetComponentInParent<LightSource>().GetLightLuminosity().ToString("F2");
    }
}
