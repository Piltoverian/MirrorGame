using System.Collections.Generic;
using UnityEngine;

public class LightReceiver : LightUtility
{
    private List<LightRayData> hit = new List<LightRayData>();

    [SerializeField] private float lumosityToOpen = 10f;
    [SerializeField] private bool requireSpecificColor = false;
    [SerializeField] private LightColorChannel requiredColor = LightColorChannel.White;
    [SerializeField] private bool requireExactColor = true;
    [SerializeField] private float requiredHoldTime = 0f;
    [SerializeField] private bool countsForWin = true;

    private int lastEvaluationFrame = -1;
    private float satisfiedTimer;
    private bool cachedOpened;

    public override void OnLightGraphClear()
    {
        hit.Clear();
        lastEvaluationFrame = -1;
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        for (int i = 0; i < hit.Count; i++)
        {
            if (IsSameRay(hit[i], lightRayData))
            {
                hit[i] = lightRayData;
                return;
            }
        }

        hit.Add(lightRayData);
    }

    public float GetTotalLuminosity()
    {
        float lightLuminositySum = 0f;
        foreach (var rayData in hit)
        {
            lightLuminositySum += rayData.lightluminosity;
        }
        return lightLuminositySum;
    }

    public float GetMatchingLuminosity()
    {
        float lightLuminositySum = 0f;
        foreach (var rayData in hit)
        {
            if (DoesRayColorMatch(rayData))
            {
                lightLuminositySum += rayData.lightluminosity;
            }
        }

        return lightLuminositySum;
    }

    public bool IsOpened()
    {
        EvaluateOpenedState();
        return cachedOpened;
    }

    public float GetLumosityToOpen()
    {
        return lumosityToOpen;
    }

    public bool CountsForWin()
    {
        return countsForWin;
    }

    public LightColorChannel GetRequiredColor()
    {
        return requiredColor;
    }

    public bool RequiresSpecificColor()
    {
        return requireSpecificColor;
    }

    public bool RequiresExactColor()
    {
        return requireExactColor;
    }

    public void ConfigureRequirement(float requiredLuminosity, bool requireColor, LightColorChannel color, bool exactColor)
    {
        lumosityToOpen = requiredLuminosity;
        requireSpecificColor = requireColor;
        requiredColor = color;
        requireExactColor = exactColor;
        lastEvaluationFrame = -1;
    }

    public void SetCountsForWin(bool shouldCountForWin)
    {
        countsForWin = shouldCountForWin;
    }

    private void EvaluateOpenedState()
    {
        if (lastEvaluationFrame == Time.frameCount)
        {
            return;
        }

        float luminosity = requireSpecificColor ? GetMatchingLuminosity() : GetTotalLuminosity();
        bool isSatisfied = luminosity >= lumosityToOpen;

        if (isSatisfied)
        {
            satisfiedTimer += Time.deltaTime;
        }
        else
        {
            satisfiedTimer = 0f;
        }

        cachedOpened = isSatisfied && satisfiedTimer >= requiredHoldTime;
        lastEvaluationFrame = Time.frameCount;
    }

    private bool DoesRayColorMatch(LightRayData lightRayData)
    {
        if (!requireSpecificColor)
        {
            return true;
        }

        return LightColorHelper.Matches(lightRayData.lightColor, requiredColor, requireExactColor);
    }

    private bool IsSameRay(LightRayData a, LightRayData b)
    {
        return a.emitObject == b.emitObject
            && a.emitpos == b.emitpos
            && a.raydir == b.raydir;
    }
}
