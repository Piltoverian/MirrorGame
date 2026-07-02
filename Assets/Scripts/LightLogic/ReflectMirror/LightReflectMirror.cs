using System.Collections.Generic;
using UnityEngine;

public class LightReflectMirror : LightUtility
{
    private struct EmittedRay
    {
        public Vector2 position;
        public Vector3 direction;
    }

    private List<EmittedRay> currentEmittedRays = new List<EmittedRay>();

    [Header("Anti-Loop Settings")]
    [SerializeField] private float angleTolerance = 1f;
    [SerializeField] private float posTolerance = 0.1f;
    [SerializeField] private int maxBounces = 5;

    public override void OnLightHit(LightRayData lightRayData)
    {

        if (Vector3.Dot(lightRayData.raydir, transform.up) > 0.01f)
        {
            return;
        }

        Vector3 reflectDir = CalculateReflectDir(lightRayData);

        Vector2 reflectPos = lightRayData.hitpos;

        int sameRayCount = 0;
        for (int i = 0; i < currentEmittedRays.Count; i++)
        {
            float posDiff = Vector2.Distance(currentEmittedRays[i].position, reflectPos);
            float angleDiff = Vector3.Angle(currentEmittedRays[i].direction, reflectDir);

            if (posDiff <= posTolerance && angleDiff <= angleTolerance)
            {
                sameRayCount++;
            }
        }

        if (sameRayCount >= maxBounces)
        {
            return;
        }

        currentEmittedRays.Add(new EmittedRay { position = reflectPos, direction = reflectDir });

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        int[] oldLayers = new int[allChildren.Length];
        for (int i = 0; i < allChildren.Length; i++)
        {
            oldLayers[i] = allChildren[i].gameObject.layer;
            allChildren[i].gameObject.layer = 2; 
        }

        var lightdata = LightRayHelper.LightEmit(gameObject, CalculateReflectDir(lightRayData), lightRayData.lightdiagramindex, lightRayData.lightluminosity,lightRayData.hitpos);

        for (int i = 0; i < allChildren.Length; i++)
        {
            allChildren[i].gameObject.layer = oldLayers[i];
        }

        if (lightdata != null)
        {
            LightRendererPipeLine lightRendererPipeLine = FindAnyObjectByType<LightRendererPipeLine>();
            if (lightRendererPipeLine != null)
            {
                lightRendererPipeLine.AddAnEdgeToLightGraph(lightdata);
            }
            else
            {
                Debug.LogError("No LightRendererPipeLine found in the scene.");
            }
        }
        else
        {
            Debug.LogError("Failed to emit reflected light ray.");
        }
    }

    public Vector3 CalculateReflectDir(LightRayData incomingRay)
    {
        if (incomingRay.hitCollider == null)
        {
            Debug.LogError("Incoming ray does not hit any collider.");
            return Vector3.zero;
        }
        // Calculate the reflection direction using the formula: R = D - 2 * (D . N) * N
        Vector3 normal = transform.up;
        return incomingRay.raydir - 2 * Vector3.Dot(incomingRay.raydir, normal) * normal;
    }

    public override void OnLightGraphClear()
    {
        currentEmittedRays.Clear();
    }
}

