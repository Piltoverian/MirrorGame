using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LightRayData//use this to store the data of a light ray used to caculate the the merge of light ray and redraw later
{
    public Collider2D hitCollider;//use for graph logic
    public GameObject emitObject;//use for graph logic
    public float lightluminosity;//use for calculate the luminosity of the light ray
    public System.Collections.Generic.List<int> lightdiagramindex;//use for graph logic
    public Vector2 hitpos;//use for draw the light ray
    public Vector2 emitpos;//use for draw the light ray
    public Vector3 raydir;//use for draw the light ray
    public LightColorChannel lightColor;
    public float lightSpeed;
    public float pathDelay;
    public float segmentTravelTime;
    public float distance;

    public LightRayData()
    {
        hitCollider = null;
        emitObject = null;
        lightluminosity = 0f;
        lightdiagramindex = new System.Collections.Generic.List<int>();
        hitpos = Vector2.zero;
        emitpos = Vector2.zero;
        raydir = Vector3.zero;
        lightColor = LightColorChannel.White;
        lightSpeed = 0f;
        pathDelay = 0f;
        segmentTravelTime = 0f;
        distance = 0f;
    }   

    public float GetEndDelay()
    {
        return pathDelay + segmentTravelTime;
    }
}

public static class LightRayHelper
{
    public static int LightCollisionMask = (1 << 6) | (1 << 7) | (1 << 8);
    public static float MaxDistance = 100f;
    public static float DefaultLightSpeed = 20f;

    public static LightRayData LightEmit(GameObject ultility, Vector3 lightDir, List<int> lightsourceindex, float LightLuminosity,Vector3 emitpoint)
    {
        return LightEmit(
            ultility,
            lightDir,
            lightsourceindex,
            LightLuminosity,
            emitpoint,
            LightColorChannel.White,
            DefaultLightSpeed,
            0f
        );
    }

    public static LightRayData LightEmit(
        GameObject ultility,
        Vector3 lightDir,
        List<int> lightsourceindex,
        float lightLuminosity,
        Vector3 emitpoint,
        LightColorChannel lightColor,
        float lightSpeed,
        float pathDelay
    )
    {
        LightRayData lightRayData = new LightRayData();
        lightRayData.emitpos = emitpoint;
        lightRayData.emitObject = ultility.gameObject;
        lightRayData.lightdiagramindex.AddRange(lightsourceindex);
        lightRayData.lightluminosity = lightLuminosity;
        lightRayData.lightColor = lightColor;
        lightRayData.lightSpeed = lightSpeed > 0f ? lightSpeed : DefaultLightSpeed;
        lightRayData.pathDelay = Mathf.Max(0f, pathDelay);
        lightRayData.raydir = lightDir.sqrMagnitude > 0f ? lightDir.normalized : Vector3.right;

        Ray2D ray = new Ray2D(emitpoint, lightRayData.raydir);
        RaycastHit2D raycastHit2D = Physics2D.Raycast(ray.origin, ray.direction, MaxDistance, LightCollisionMask);

        if (raycastHit2D)
        {
            lightRayData.hitpos = raycastHit2D.point + raycastHit2D.normal* 0.0001f;
            lightRayData.hitCollider = raycastHit2D.collider;
            ApplyTravelData(lightRayData);

            LightUtility hitLightUltility = lightRayData.hitCollider.GetComponent<LightUtility>();
            if (hitLightUltility == null)
            {
                hitLightUltility = lightRayData.hitCollider.GetComponentInParent<LightUtility>();
            }

            if (hitLightUltility != null)
            {
                hitLightUltility.OnLightHit(lightRayData);
            }
            else
            {
                //hit so block light
                lightRayData.hitCollider=raycastHit2D.collider;
                lightRayData.hitpos = raycastHit2D.point + raycastHit2D.normal* 0.0001f;
            }
        }
        else
        {
            lightRayData.hitpos = emitpoint + lightRayData.raydir * MaxDistance;
            lightRayData.hitCollider = null;
            ApplyTravelData(lightRayData);
        }

        return lightRayData;
    }

    public static LightRayData LightEmit(LightRayData lightRayData)
    {
        return LightEmit(
            lightRayData.emitObject,
            lightRayData.raydir,
            lightRayData.lightdiagramindex,
            lightRayData.lightluminosity,
            lightRayData.emitpos,
            lightRayData.lightColor,
            lightRayData.lightSpeed,
            lightRayData.pathDelay
        );
    }

    private static void ApplyTravelData(LightRayData lightRayData)
    {
        lightRayData.distance = Vector2.Distance(lightRayData.emitpos, lightRayData.hitpos);
        lightRayData.segmentTravelTime = lightRayData.lightSpeed > 0f
            ? lightRayData.distance / lightRayData.lightSpeed
            : 0f;
    }
}
