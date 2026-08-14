using TMPro;
using UnityEngine;

public class DoorLuminosityDisplay : MonoBehaviour
{
    [SerializeField] private Color openedColor;
    [SerializeField] private Color closedColor;
    [SerializeField] private bool showCurrentLuminosity = false;

    private TextMeshPro textMesh;
    private LightReceiver receiver;

    private void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        receiver = GetComponentInParent<LightReceiver>();

        if (receiver != null)
        {
            textMesh.text = receiver.GetLumosityToOpen().ToString("F2");
        }
    }

    private void Update()
    {
        if (receiver == null || textMesh == null)
        {
            return;
        }

        if (showCurrentLuminosity)
        {
            float currentLuminosity = receiver.RequiresSpecificColor()
                ? receiver.GetMatchingLuminosity()
                : receiver.GetTotalLuminosity();

            textMesh.text = currentLuminosity.ToString("F1") + "/" + receiver.GetLumosityToOpen().ToString("F1");
        }

        if (receiver.IsOpened())
        {
            textMesh.color = openedColor;
        }
        else
        {
            textMesh.color = closedColor;
        }
    }
}
