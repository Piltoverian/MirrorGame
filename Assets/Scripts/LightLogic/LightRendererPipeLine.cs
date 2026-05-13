using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LightRendererPipeLine : MonoBehaviour
{
    List<LightRayData> lightTotalGraph = new List<LightRayData>();
    public List<LightRayData> LightCalculatePhase()
    {
        lightTotalGraph.Clear();
        // Perform light calculation logic here
        var lightSources = FindObjectsByType<LightSource>(FindObjectsSortMode.None);

        foreach (var lightSource in lightSources)
        {
            var lightindexlist= new List<int>();
            lightindexlist.Add(lightSource.GetLightSourceIndex());
            var lightRayData = LightRayHelper.LightEmit(lightSource, lightSource.transform.right,lightindexlist, lightSource.GetLightLuminosity(),lightSource.transform.position);
            lightTotalGraph.Add(lightRayData);
        }
        
        return lightTotalGraph;
    }

    public void LightRenderPhase(List<LightRayData> lightRayDataList)
    {
        Debug.Log("LightRenderPhase: " + lightRayDataList.Count + " light rays to render.");
        // Perform light rendering logic here using the calculated light ray data
        foreach (var lightRayData in lightRayDataList)
        {
            // Example: Draw a line representing the light ray
           Debug.DrawLine(lightRayData.emitpos, lightRayData.hitpos, Color.yellow);
        }
    }

    public void AddAnEdgeToLightGraph(LightRayData lightRayData)
    {
        lightTotalGraph.Add(lightRayData);
        Debug.Log("Added a new edge to the light graph. Total edges: " + lightTotalGraph.Count);
    }   


    public void Update()
    {
        LightCalculatePhase();
        
    }

    public void LateUpdate()
    {
        LightRenderPhase(lightTotalGraph);
    }
}
