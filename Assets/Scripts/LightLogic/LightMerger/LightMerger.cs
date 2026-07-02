using System.Collections.Generic;
using UnityEngine;

public class LightMerger : LightUtility
{
    [SerializeField] private GameObject lightoutput;
    [SerializeField] private LightRayData currentRayData;
    private List<LightRayData> hit = new List<LightRayData>();

    private bool hasFrontHit = false;
    private bool hasBackHit = false;

    public void Awake()
    {
        ClearRayData();
    }

    public void ClearRayData()
    {
        hit.Clear();
        currentRayData = new LightRayData();
        currentRayData.emitObject =lightoutput;
        currentRayData.lightdiagramindex = new System.Collections.Generic.List<int>();
        currentRayData.lightluminosity = 0f;
        currentRayData.emitpos = lightoutput.transform.position;
        currentRayData.raydir = lightoutput.transform.up;
    }

    public override void OnLightGraphClear()
    {
        ClearRayData();
    }

    public override void OnLightHit(LightRayData lightRayData)
    {

        for(int i = 0; i < lightRayData.lightdiagramindex.Count; i++)
        {
            if (!currentRayData.lightdiagramindex.Contains(lightRayData.lightdiagramindex[i]))
            {
                currentRayData.lightdiagramindex.Add(lightRayData.lightdiagramindex[i]);
            }
        }
        bool isNewHit = true;
        for (int i = 0; i < hit.Count; i++)
        {
            if (hit[i].emitObject == lightRayData.emitObject && hit[i].emitpos == lightRayData.emitpos && hit[i].raydir == lightRayData.raydir)
            {
                hit[i] = lightRayData;
                isNewHit = false;
                break;
            }
        }
        if (isNewHit)
        {
            hit.Add(lightRayData);
        }
        float lightLuminositySum = 0f;
        foreach (var rayData in hit)
        {
            lightLuminositySum += rayData.lightluminosity;
        }
      currentRayData.lightluminosity = lightLuminositySum;
      currentRayData = LightRayHelper.LightEmit(currentRayData);
      var lightRendererPipeLine = FindAnyObjectByType<LightRendererPipeLine>();
      if (lightRendererPipeLine != null)
      {
              lightRendererPipeLine.UpdateLightGraph(currentRayData);
      }
      else
      {
              Debug.LogError("No LightRendererPipeLine found in the scene.");
      }
    }

    
}
