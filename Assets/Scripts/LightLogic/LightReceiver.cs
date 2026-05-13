using System.Collections.Generic;
using UnityEngine;

public class LightReceiver : LightUtility
{
    private List<LightRayData> hit = new List<LightRayData>();
    [SerializeField] private float lumosityToOpen = 10f;
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

    public bool IsOpened()
    {
        return GetTotalLuminosity() >= lumosityToOpen;
    }
}
