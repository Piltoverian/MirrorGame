using System.Collections.Generic;
using UnityEngine;

public class LightSplitter : LightUtility
{
    [SerializeField] private GameObject lightoutput;
    [SerializeField] private float splitAngle = 30f;
    [SerializeField] private List<LightRayData> hit = new List<LightRayData>();
    LightRayData lightRayData1;
    LightRayData lightRayData2;
    private bool hasEmitted = false;

    public void Awake()
    {
        ClearRayData();
    }

    public void ClearRayData()
    {
        hit.Clear();
        lightRayData1 = new LightRayData();
        lightRayData2 = new LightRayData();
        lightRayData1.emitObject = lightoutput;
        lightRayData1.emitpos = lightoutput.transform.position;
        lightRayData1.lightdiagramindex = new System.Collections.Generic.List<int>();
        lightRayData1.raydir = Quaternion.Euler(0, 0, splitAngle) * lightoutput.transform.up;
        lightRayData2.emitObject = lightoutput;
        lightRayData2.emitpos = lightoutput.transform.position;
        lightRayData2.lightdiagramindex = new System.Collections.Generic.List<int>();
        lightRayData2.raydir = Quaternion.Euler(0, 0, -splitAngle) * lightoutput.transform.up;
        hasEmitted = false;
    }

    public override void OnLightGraphClear()
    {
        ClearRayData();
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        bool foundMatch = false;
        for (int i = 0; i < hit.Count; i++)
        {
            if (hit[i].emitObject == lightRayData.emitObject)
            {
                hit[i] = lightRayData;
                foundMatch = true;
                break;
            }
        }
        if (!foundMatch)
        {
            hit.Add(lightRayData);
        }

        float lightLuminositySum = 0f;
        Color ColorSum = Color.black;
        foreach (var rayData in hit)
        {
            lightLuminositySum += rayData.lightluminosity;
            ColorSum += rayData.Color;
        }

        lightRayData1.lightluminosity = lightLuminositySum * 0.5f;
        lightRayData2.lightluminosity = lightLuminositySum * 0.5f;
        lightRayData1.Color = ColorSum * 0.5f;
        lightRayData2.Color = ColorSum * 0.5f;

        if (!hasEmitted)
        {
            hasEmitted = true;
            lightRayData1 = LightRayHelper.LightEmit(lightRayData1.emitObject, lightRayData1.raydir, lightRayData1.lightdiagramindex, lightRayData1.lightluminosity, lightRayData1.emitpos, lightRayData1.Color);
            lightRayData2 = LightRayHelper.LightEmit(lightRayData2.emitObject, lightRayData2.raydir, lightRayData2.lightdiagramindex, lightRayData2.lightluminosity, lightRayData2.emitpos, lightRayData2.Color);
        }
        else
        {
            if (lightRayData1.visualRay != null)
                lightRayData1.visualRay.SetColor(lightRayData1.Color);
            if (lightRayData2.visualRay != null)
                lightRayData2.visualRay.SetColor(lightRayData2.Color);

            if (lightRayData1.hitCollider != null && lightRayData1.hitCollider.TryGetComponent(out LightUtility nextTarget1))
            {
                nextTarget1.OnLightHit(lightRayData1);
            }
            if (lightRayData2.hitCollider != null && lightRayData2.hitCollider.TryGetComponent(out LightUtility nextTarget2))
            {
                nextTarget2.OnLightHit(lightRayData2);
            }
        }
    }
}
