using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LightSplitter : LightUtility
{
    [SerializeField] private GameObject lightoutput;
    [SerializeField] private float splitAngle = 30f;
    [SerializeField]private List<LightRayData> hit=new List<LightRayData>();
    LightRayData lightRayData1;
    LightRayData lightRayData2;
    public void Awake()
    {
        ClearRayData();
    }

    public void ClearRayData()
    {
        hit.Clear();
        lightRayData1 =new LightRayData();
        lightRayData2=new LightRayData();
        lightRayData1.emitObject = lightoutput;
        lightRayData1.emitpos = lightoutput.transform.position;
        lightRayData1.lightdiagramindex = new System.Collections.Generic.List<int>();
        lightRayData1.raydir = Quaternion.Euler(0, 0, splitAngle) * lightoutput.transform.up;
        lightRayData2.emitObject = lightoutput;
        lightRayData2.emitpos = lightoutput.transform.position;
        lightRayData2.lightdiagramindex = new System.Collections.Generic.List<int>();
        lightRayData2.raydir = Quaternion.Euler(0, 0, -splitAngle) * lightoutput.transform.up;
    }

    public override void OnLightGraphClear()
    {
        ClearRayData();
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        for (int i = 0; i < lightRayData.lightdiagramindex.Count; i++)
        {
            if (!lightRayData1.lightdiagramindex.Contains(lightRayData.lightdiagramindex[i]))
            {
                lightRayData1.lightdiagramindex.Add(lightRayData.lightdiagramindex[i]);
                lightRayData2.lightdiagramindex.Add(lightRayData.lightdiagramindex[i]);
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
        lightRayData1.lightluminosity = lightLuminositySum * 0.5f;
        lightRayData2.lightluminosity = lightLuminositySum * 0.5f;
        lightRayData1=LightRayHelper.LightEmit(lightRayData1);
        lightRayData2=LightRayHelper.LightEmit(lightRayData2);
        var lightRendererPipeLine = FindAnyObjectByType<LightRendererPipeLine>();
        if (lightRendererPipeLine != null)
        {
            lightRendererPipeLine.UpdateLightGraph(lightRayData1);
            lightRendererPipeLine.UpdateLightGraph(lightRayData2);
        }
        else
        {
            Debug.LogError("No LightRendererPipeLine found in the scene.");
        }
    }
}
