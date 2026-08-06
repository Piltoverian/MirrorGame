using UnityEngine;

public class LightRay : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField] private float luminosity;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetLuminosity(float value)
    {
        if (lineRenderer != null && lineRenderer.material != null)
        {
            lineRenderer.material.SetFloat("_Luminosity", value);
            luminosity = value;
        }
    }

    public void SetColor(Color color)
    {
        if (lineRenderer!=null && lineRenderer.material != null)
        {
            lineRenderer.material.SetColor("_LightColor", color);
        }
    }
}
