using System.Collections.Generic;
using UnityEngine;

public class LightSource : LightUtility
{
    [SerializeField] int lightSourceIndex;
    [SerializeField] float lightLuminosity; 
    

    private void OnValidate()
    {
        //find the first missing positive number in LightSoureceIndex
        List<LightSource> lightSources = new List<LightSource>(FindObjectsByType<LightSource>(FindObjectsSortMode.None));
        List<int> lightSourceIndices = new List<int>(lightSources.Count);
        for (int i = 0; i < lightSources.Count; i++)
        {
            lightSourceIndices.Add(lightSources[i].lightSourceIndex);
        }
        lightSourceIndices.Sort(); 
        for (int i = 0; i < lightSourceIndices.Count; i++)
        {
            if (lightSourceIndices[i]-1 != i)
            {
                lightSourceIndex = i+1;
                return;
            }
        }
        lightSourceIndex = lightSourceIndices.Count+1;    
    }

    public int GetLightSourceIndex()
    {
        return lightSourceIndex;
    }

    public float GetLightLuminosity()
    {
        return lightLuminosity;
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        
    }

    public override void OnLightGraphClear()
    {
        
    }
}
