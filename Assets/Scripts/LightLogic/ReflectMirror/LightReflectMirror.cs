using UnityEngine;

public class LightReflectMirror : LightUtility
{
    public override void OnLightHit(LightRayData lightRayData)
    {
        var lightdata=LightRayHelper.LightEmit(gameObject, CalculateReflectDir(lightRayData), lightRayData.lightdiagramindex, lightRayData.lightluminosity,lightRayData.hitpos);
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
        
    }
}
