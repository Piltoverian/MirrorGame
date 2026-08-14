using System.Collections.Generic;
using UnityEngine;

public class LightSource : LightUtility
{
    [SerializeField] private int lightSourceIndex;
    [SerializeField] private float lightLuminosity = 10f;
    [SerializeField] private LightColorChannel lightColor = LightColorChannel.White;
    [Tooltip("Use <= 0 to inherit the speed configured on LightRendererPipeLine.")]
    [SerializeField] private float lightSpeedOverride = -1f;
    

    private void OnValidate()
    {
        //find the first missing positive number in LightSoureceIndex
        List<LightSource> lightSources = new List<LightSource>(FindObjectsByType<LightSource>());
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

    public LightColorChannel GetLightColor()
    {
        return lightColor;
    }

    public float GetLightSpeed(float fallbackSpeed)
    {
        return lightSpeedOverride > 0f ? lightSpeedOverride : fallbackSpeed;
    }

    public void Configure(LightColorChannel newColor, float newLuminosity)
    {
        lightColor = newColor;
        lightLuminosity = newLuminosity;
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        
    }

    public override void OnLightGraphClear()
    {
        
    }
}
