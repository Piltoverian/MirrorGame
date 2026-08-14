using System.Collections.Generic;
using UnityEngine;

public class LightSplitter : LightUtility
{
    [SerializeField] private GameObject lightoutput;
    [SerializeField] private float splitAngle = 30f;
    [SerializeField] private bool splitIntoRgbComponents = false;
    [SerializeField] private float splitDelay = 0f;
    [SerializeField] private int minimumInputRays = 1;
    [SerializeField] private Transform[] configuredOutputs;
    [SerializeField] private Vector2[] localOutputDirections;

    [SerializeField]private List<LightRayData> hit=new List<LightRayData>();

    public void Awake()
    {
        ClearRayData();
    }

    public void ClearRayData()
    {
        hit.Clear();
    }

    public override void OnLightGraphClear()
    {
        ClearRayData();
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
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
        List<int> diagramIndexes = new List<int>();

        foreach (var rayData in hit)
        {
            lightLuminositySum += rayData.lightluminosity;
            latestInputEndDelay = Mathf.Max(latestInputEndDelay, rayData.GetEndDelay());

            for (int i = 0; i < rayData.lightdiagramindex.Count; i++)
            {
                if (!diagramIndexes.Contains(rayData.lightdiagramindex[i]))
                {
                    diagramIndexes.Add(rayData.lightdiagramindex[i]);
                }
            }
        }

        List<LightColorChannel> outputColors = splitIntoRgbComponents
            ? LightColorHelper.SplitBaseColors(LightColorHelper.Merge(hit))
            : BuildRepeatedColorList(LightColorHelper.Merge(hit), GetOutputCount());

        int outputCount = outputColors.Count;
        float outputLuminosity = outputCount > 0 ? lightLuminositySum / outputCount : lightLuminositySum;
        var lightRendererPipeLine = FindAnyObjectByType<LightRendererPipeLine>();
        if (lightRendererPipeLine != null)
        {
            for (int i = 0; i < outputCount; i++)
            {
                Transform outputTransform = GetConfiguredOutput(i);
                GameObject emitObject = outputTransform != null ? outputTransform.gameObject : GetDefaultOutputObject();
                Vector3 emitPosition = outputTransform != null ? outputTransform.position : GetDefaultOutputPosition();
                Vector3 direction = GetOutputDirection(i, outputCount);

                LightRayData emittedData = LightRayHelper.LightEmit(
                    emitObject,
                    direction,
                    diagramIndexes,
                    outputLuminosity,
                    emitPosition,
                    outputColors[i],
                    lightRayData.lightSpeed,
                    latestInputEndDelay + splitDelay
                );

                lightRendererPipeLine.UpdateLightGraph(emittedData);
            }
        }
        else
        {
            Debug.LogError("No LightRendererPipeLine found in the scene.");
        }
    }

    private int GetOutputCount()
    {
        if (configuredOutputs != null && configuredOutputs.Length > 0)
        {
            return configuredOutputs.Length;
        }

        if (localOutputDirections != null && localOutputDirections.Length > 0)
        {
            return localOutputDirections.Length;
        }

        return splitIntoRgbComponents ? 3 : 2;
    }

    private List<LightColorChannel> BuildRepeatedColorList(LightColorChannel color, int count)
    {
        List<LightColorChannel> colors = new List<LightColorChannel>();
        for (int i = 0; i < Mathf.Max(1, count); i++)
        {
            colors.Add(color);
        }

        return colors;
    }

    private Transform GetConfiguredOutput(int index)
    {
        if (configuredOutputs == null || configuredOutputs.Length == 0)
        {
            return null;
        }

        return configuredOutputs[Mathf.Clamp(index, 0, configuredOutputs.Length - 1)];
    }

    private GameObject GetDefaultOutputObject()
    {
        return lightoutput != null ? lightoutput : gameObject;
    }

    private Vector3 GetDefaultOutputPosition()
    {
        return lightoutput != null ? lightoutput.transform.position : transform.position;
    }

    private Vector3 GetOutputDirection(int index, int outputCount)
    {
        Transform outputTransform = GetConfiguredOutput(index);
        if (outputTransform != null)
        {
            return outputTransform.up;
        }

        if (localOutputDirections != null && index < localOutputDirections.Length && localOutputDirections[index].sqrMagnitude > 0f)
        {
            Vector2 localDirection = localOutputDirections[index].normalized;
            return transform.TransformDirection(new Vector3(localDirection.x, localDirection.y, 0f));
        }

        Transform outputBase = lightoutput != null ? lightoutput.transform : transform;

        if (splitIntoRgbComponents && outputCount >= 3)
        {
            float rgbAngle = index == 0 ? splitAngle : index == 1 ? 0f : -splitAngle;
            return Quaternion.Euler(0f, 0f, rgbAngle) * outputBase.up;
        }

        float directionSign = index % 2 == 0 ? 1f : -1f;
        return Quaternion.Euler(0f, 0f, splitAngle * directionSign) * outputBase.up;
    }
}
