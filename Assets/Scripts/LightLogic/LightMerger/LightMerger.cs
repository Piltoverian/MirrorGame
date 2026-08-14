using System.Collections.Generic;
using UnityEngine;

public class LightMerger : LightUtility
{
    [SerializeField] private GameObject lightoutput;
    [SerializeField] private LightRayData currentRayData;
    [SerializeField] private bool mergeColorChannels = true;
    [SerializeField] private int minimumInputRays = 2;
    [SerializeField] private float mergeDelay = 0f;

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
        currentRayData.emitObject = GetOutputObject();
        currentRayData.lightdiagramindex = new System.Collections.Generic.List<int>();
        currentRayData.lightluminosity = 0f;
        currentRayData.lightColor = LightColorChannel.None;
        currentRayData.emitpos = GetOutputPosition();
        currentRayData.raydir = GetOutputDirection();
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

        if (hit.Count < Mathf.Max(1, minimumInputRays))
        {
            return;
        }

        float lightLuminositySum = 0f;
        float latestInputEndDelay = 0f;

        foreach (var rayData in hit)
        {
            lightLuminositySum += rayData.lightluminosity;
            latestInputEndDelay = Mathf.Max(latestInputEndDelay, rayData.GetEndDelay());
        }

      currentRayData.emitObject = GetOutputObject();
      currentRayData.emitpos = GetOutputPosition();
      currentRayData.raydir = GetOutputDirection();
      currentRayData.lightluminosity = lightLuminositySum;
      currentRayData.lightColor = mergeColorChannels ? LightColorHelper.Merge(hit) : lightRayData.lightColor;
      currentRayData.lightSpeed = lightRayData.lightSpeed;
      currentRayData.pathDelay = latestInputEndDelay + mergeDelay;
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

    private GameObject GetOutputObject()
    {
        return lightoutput != null ? lightoutput : gameObject;
    }

    private Vector3 GetOutputPosition()
    {
        return lightoutput != null ? lightoutput.transform.position : transform.position;
    }

    private Vector3 GetOutputDirection()
    {
        return lightoutput != null ? lightoutput.transform.up : transform.up;
    }
    
}
