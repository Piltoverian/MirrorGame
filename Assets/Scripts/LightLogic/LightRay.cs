using UnityEngine;

public class LightRay : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetLuminosity(float value)
    {
        if (lineRenderer != null && lineRenderer.material != null)
        {
            lineRenderer.material.SetFloat("_Luminosity", value);
        }
    }
}
