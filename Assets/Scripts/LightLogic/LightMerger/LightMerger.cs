using System.Collections.Generic;
using UnityEngine;

public class LightMerger : LightUtility
{
    [SerializeField] private GameObject lightoutput;
    [SerializeField] private LightRayData currentRayData;
    private List<LightRayData> hit = new List<LightRayData>();

    private bool hasFrontHit = false;
    private bool hasBackHit = false;
    private bool hasEmitted = false;

    public void Awake()
    {
        ClearRayData();
    }

    public void ClearRayData()
    {
        hit.Clear();
        currentRayData = new LightRayData();
        currentRayData.emitObject = lightoutput;
        currentRayData.lightdiagramindex = new System.Collections.Generic.List<int>();
        currentRayData.lightluminosity = 0f;
        currentRayData.emitpos = lightoutput.transform.position;
        currentRayData.raydir = lightoutput.transform.up;
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
        Color colorSum = Color.black;
        foreach (var rayData in hit)
        {
            lightLuminositySum += rayData.lightluminosity;
            colorSum += rayData.Color;
        }

        currentRayData.lightluminosity = lightLuminositySum;
        currentRayData.Color = colorSum;

        if (!hasEmitted)
        {
            hasEmitted = true;
            currentRayData = LightRayHelper.LightEmit(currentRayData.emitObject, currentRayData.raydir, currentRayData.lightdiagramindex, currentRayData.lightluminosity, currentRayData.emitpos, currentRayData.Color);
        }
        else
        {
            if (currentRayData.visualRay != null)
            {
                currentRayData.visualRay.SetColor(colorSum);
            }
            if (currentRayData.hitCollider != null && currentRayData.hitCollider.TryGetComponent(out LightUtility nextTarget))
            {
                nextTarget.OnLightHit(currentRayData);
            }
        }
    }
}
