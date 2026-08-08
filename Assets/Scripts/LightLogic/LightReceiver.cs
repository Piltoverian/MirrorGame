using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightReceiver : LightUtility
{
    private List<LightRayData> hit = new List<LightRayData>();
    [SerializeField] private float lumosityToOpen = 10f;
    [SerializeField] private Color colorToOpen= Color.white;
    [SerializeField] private float colorTolerance = 0.1f;
    public override void OnLightGraphClear()
    {
        hit.Clear();
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        if (!hit.Contains(lightRayData))
        {
            hit.Add(lightRayData);
        }
        else
        {
            for (int i = 0; i < hit.Count; i++)
            {
                if (hit[i].emitObject == lightRayData.emitObject && hit[i].emitpos == lightRayData.emitpos && hit[i].raydir == lightRayData.raydir)
                {
                    hit[i] = lightRayData;
                    break;
                }
            }
        }
    }

    public float GetTotalLuminosity()
    {
        float lightLuminositySum = 0f;
        foreach (var rayData in hit)
        {
            lightLuminositySum += rayData.lightluminosity;
        }
        return lightLuminositySum;
    }

    public Color GetTotalColor()
    {
        Color colorSum = Color.black;
        foreach (var rayData in hit)
        {
            colorSum += rayData.Color;
        }
        return colorSum;
    }

    public bool IsOpened()
    {
        Color offsetColor= GetTotalColor() - colorToOpen;
        bool isColorMatch = Mathf.Abs(offsetColor.r) <= colorTolerance && Mathf.Abs(offsetColor.g) <= colorTolerance && Mathf.Abs(offsetColor.b) <= colorTolerance;
        return GetTotalLuminosity() >= lumosityToOpen && isColorMatch;
    }

    public float GetLumosityToOpen()
    {
        return lumosityToOpen;
    }
}
