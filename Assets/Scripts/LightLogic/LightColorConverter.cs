using System.Collections.Generic;
using UnityEngine;

public class LightColorConverter : LightUtility
{
    [SerializeField] private Transform lightOutput;
    [SerializeField] private LightColorChannel outputColor = LightColorChannel.Red;
    [SerializeField] private bool keepIncomingDirection = true;
    [SerializeField] private float outputLuminosityMultiplier = 1f;
    [SerializeField] private float conversionDelay = 0f;

    private readonly List<LightRayData> hit = new List<LightRayData>();

    public override void OnLightGraphClear()
    {
        hit.Clear();
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        UpsertHit(lightRayData);

        Transform output = lightOutput != null ? lightOutput : transform;
        Vector3 direction = keepIncomingDirection ? lightRayData.raydir : output.up;
        Vector3 emitPosition = lightOutput != null ? lightOutput.position : lightRayData.hitpos;

        LightRayData convertedRay = EmitConvertedRayIgnoringSelf(lightRayData, output, direction, emitPosition);

        LightRendererPipeLine lightRendererPipeLine = FindAnyObjectByType<LightRendererPipeLine>();
        if (lightRendererPipeLine != null)
        {
            lightRendererPipeLine.UpdateLightGraph(convertedRay);
        }
        else
        {
            Debug.LogError("No LightRendererPipeLine found in the scene.");
        }
    }

    private LightRayData EmitConvertedRayIgnoringSelf(
        LightRayData sourceRay,
        Transform output,
        Vector3 direction,
        Vector3 emitPosition
    )
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        int[] oldLayers = new int[allChildren.Length];

        for (int i = 0; i < allChildren.Length; i++)
        {
            oldLayers[i] = allChildren[i].gameObject.layer;
            allChildren[i].gameObject.layer = 2;
        }

        LightRayData convertedRay = LightRayHelper.LightEmit(
            output.gameObject,
            direction,
            sourceRay.lightdiagramindex,
            sourceRay.lightluminosity * outputLuminosityMultiplier,
            emitPosition,
            outputColor,
            sourceRay.lightSpeed,
            sourceRay.GetEndDelay() + conversionDelay
        );

        for (int i = 0; i < allChildren.Length; i++)
        {
            allChildren[i].gameObject.layer = oldLayers[i];
        }

        return convertedRay;
    }

    private void UpsertHit(LightRayData lightRayData)
    {
        for (int i = 0; i < hit.Count; i++)
        {
            if (hit[i].emitObject == lightRayData.emitObject && hit[i].emitpos == lightRayData.emitpos && hit[i].raydir == lightRayData.raydir)
            {
                hit[i] = lightRayData;
                return;
            }
        }

        hit.Add(lightRayData);
    }
}
