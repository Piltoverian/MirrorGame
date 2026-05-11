using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    [SerializeField] int lightsourceindex;
    [SerializeField] float LightLuminosity; 
    public LightRayData LightEmit()
    {
        LightRayData lightRayData = new LightRayData();
        lightRayData.emitpos = transform.position;
        lightRayData.emitObject = gameObject;
        lightRayData.lightdiagramindex.Add(lightsourceindex);
        lightRayData.lightluminosity = LightLuminosity;
        
        Ray ray = new Ray(transform.position, transform.right);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.NameToLayer("LightAbsorb")))
        {
            lightRayData.hitpos = hit.point;
            lightRayData.hitObject = hit.collider.gameObject;
        }
        else
        {
            lightRayData.hitpos = transform.position + transform.right * 100f;
            lightRayData.hitObject = null;
        }
        return lightRayData;
    }

    private void OnValidate()
    {
        //find the first missing positive number in LightSoureceIndex
        List<LightSource> lightSources = new List<LightSource>(FindObjectsByType<LightSource>(FindObjectsSortMode.None));
        List<int> lightSourceIndices = new List<int>(lightSources.Count);
        Debug.Log("LightSources count: " + lightSources.Count);
        for (int i = 0; i < lightSources.Count; i++)
        {
            lightSourceIndices.Add(lightSources[i].lightsourceindex);
        }
        lightSourceIndices.Sort();
        Debug.Log("LightSourceIndices: " + string.Join(", ", lightSourceIndices));  
        for (int i = 0; i < lightSourceIndices.Count; i++)
        {
            if (lightSourceIndices[i]-1 != i)
            {
                lightsourceindex = i+1;
                return;
            }
        }
        lightsourceindex = lightSourceIndices.Count+1;    
    }
}
