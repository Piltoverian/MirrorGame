using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LightRendererPipeLine : MonoBehaviour
{
    public List<LightRayData> LightCalculatePhase()
    {
        List<LightRayData> lightRayDataList = new List<LightRayData>();
        // Perform light calculation logic here
        var lightSources = FindObjectsByType<LightSource>(FindObjectsSortMode.None);

        foreach (var lightSource in lightSources)
        {
            var lightRayData = lightSource.LightEmit();
            lightRayDataList.Add(lightRayData);
        }

        return lightRayDataList;
    }

    public void LightRenderPhase(List<LightRayData> lightRayDataList)
    {
        Debug.Log("LightRenderPhase: " + lightRayDataList.Count + " light rays to render.");
        for (int i = 0; i < lightRayDataList.Count; i++)
        {
            Debug.Log($"LightRayData {i}: EmitPos={lightRayDataList[i].emitpos}, HitPos={lightRayDataList[i].hitpos}, Luminosity={lightRayDataList[i].lightluminosity}");
        }
        // Perform light rendering logic here using the calculated light ray data
        foreach (var lightRayData in lightRayDataList)
        {
            // Example: Draw a line representing the light ray
           Debug.DrawLine(lightRayData.emitpos, lightRayData.hitpos, Color.yellow);
        }
    }

    public void Update()
    {
        LightRendererPipeLine lightRendererPipeLine = FindAnyObjectByType<LightRendererPipeLine>();
        List<LightRayData> lightRayDataList = lightRendererPipeLine.LightCalculatePhase();
        lightRendererPipeLine.LightRenderPhase(lightRayDataList);
    }
}
