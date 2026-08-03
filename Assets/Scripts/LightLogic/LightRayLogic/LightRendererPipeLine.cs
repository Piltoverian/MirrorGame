using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LightRendererPipeLine : MonoBehaviour
{
    [SerializeField] GameObject lightRayPrefab;
    [SerializeField]List<LightRayData> lightTotalGraph = new List<LightRayData>();
    private List<GameObject> lightActiveRayGameObjects = new List<GameObject>();
    private List<GameObject> lightRayGameObjectsPool = new List<GameObject>();
    public List<LightRayData> LightCalculatePhase()
    {
        lightTotalGraph.Clear();
        // Perform light calculation logic here
        var lightSources = FindObjectsByType<LightSource>();

        foreach (var lightSource in lightSources)
        {
            var lightindexlist= new List<int>();
            lightindexlist.Add(lightSource.GetLightSourceIndex());
            var lightRayData = LightRayHelper.LightEmit(lightSource.gameObject, lightSource.transform.right,lightindexlist, lightSource.GetLightLuminosity(),lightSource.transform.position);
            lightTotalGraph.Add(lightRayData);
        }
        return lightTotalGraph;
    }

    public void LightRenderPhase(List<LightRayData> lightRayDataList)
    {
        // Perform light rendering logic here using the calculated light ray data
        foreach (var lightRayData in lightRayDataList)
        {
           GameObject lightRayGameObject;
            if (lightRayGameObjectsPool.Count > 0)
            {
                lightRayGameObject = lightRayGameObjectsPool[0];
                lightRayGameObjectsPool.RemoveAt(0);
                lightRayGameObject.SetActive(true);
            }
            else
            {
                lightRayGameObject = Instantiate(lightRayPrefab);
            }
            if (lightRayGameObject.TryGetComponent(out LineRenderer lineRenderer))
            { 
                lineRenderer.SetPositions(new Vector3[] { lightRayData.hitpos , lightRayData.emitpos });
                if (lightRayGameObject.TryGetComponent(out LightRay lightRay))
                {
                    lightRay.SetLuminosity(lightRayData.lightluminosity);
                }
                lightActiveRayGameObjects.Add(lightRayGameObject);
            }
            else
            {
                Debug.LogError("LightRayPrefab does not have a LineRenderer component.");
            }
        }
    }

    public void AddAnEdgeToLightGraph(LightRayData lightRayData)
    {
        lightTotalGraph.Add(lightRayData);
    }

    public void UpdateLightGraph(LightRayData lightRayData)
    {
        for (int i = 0; i < lightTotalGraph.Count; i++)
        {
            var raydata = lightTotalGraph[i];
            if (lightRayData.emitObject == raydata.emitObject && lightRayData.emitpos == raydata.emitpos && lightRayData.raydir == raydata.raydir)
            {
                lightTotalGraph[i] = lightRayData;
                return;
            }
        }
        AddAnEdgeToLightGraph(lightRayData);
    }

    public LightRayData GetLightRayData(GameObject gameObject, Vector2 emitpos,Vector3 raydir)
    {
        foreach (var lightRayData in lightTotalGraph)
        {
            if (lightRayData.emitObject == gameObject && lightRayData.emitpos == emitpos && lightRayData.raydir == raydir)
            {
                return lightRayData;
            }
        }
        return null; // Return null if no matching light ray data is found
    }

    private void LightGraphClear()
    {
        var lightUtilities = FindObjectsByType<LightUtility>();
        for(int i = 0; i < lightUtilities.Length; i++)
        {
            lightUtilities[i].OnLightGraphClear();
        }   
        for (int i = 0; i < lightActiveRayGameObjects.Count; i++)
        {
            lightActiveRayGameObjects[i].SetActive(false);
            lightRayGameObjectsPool.Add(lightActiveRayGameObjects[i]);
            lightActiveRayGameObjects.RemoveAt(i);
            i -= 1;
        }
        lightTotalGraph.Clear();
    }

    public void Update()
    {
        LightGraphClear();
        LightCalculatePhase();
    }

    public void LateUpdate()
    {
        LightRenderPhase(lightTotalGraph);
    }
}
