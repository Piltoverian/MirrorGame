using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class LightRayData//use this to store the data of a light ray used to caculate the the merge of light ray and redraw later
{
    public Collider2D hitCollider;//use for graph logic
    public GameObject emitObject;//use for graph logic
    public float lightluminosity;//use for calculate the luminosity of the light ray
    public System.Collections.Generic.List<int> lightdiagramindex;//use for graph logic
    public Vector2 hitpos;//use for draw the light ray
    public Vector2 emitpos;//use for draw the light ray
    public Vector3 raydir;//use for draw the light ray

    public LightRayData()
    {
        hitCollider = null;
        emitObject = null;
        lightluminosity = 0f;
        lightdiagramindex = new System.Collections.Generic.List<int>();
        hitpos = Vector2.zero;
        emitpos = Vector2.zero;
        raydir = Vector3.zero;
    }   
}

public static class LightRayHelper
{
    public static LightRayData LightEmit(LightUltility ultility, Vector3 lightDir, List<int> lightsourceindex, float LightLuminosity,Vector3 emitpoint)
    {
        LightRayData lightRayData = new LightRayData();
        lightRayData.emitpos = emitpoint;
        lightRayData.emitObject = ultility.gameObject;
        lightRayData.lightdiagramindex.AddRange(lightsourceindex);
        lightRayData.lightluminosity = LightLuminosity;
        lightRayData.raydir = lightDir;

        Ray2D ray = new Ray2D(emitpoint, lightDir);
        RaycastHit2D raycastHit2D;
        raycastHit2D = Physics2D.Raycast(ray.origin, ray.direction, 100f);
        if (raycastHit2D)
        {
            if (raycastHit2D.collider.gameObject == ultility.gameObject)
            {
                return null;
            }
            Debug.Log("Light ray hit: " + raycastHit2D.collider.gameObject.name);
            lightRayData.hitpos = raycastHit2D.point + raycastHit2D.normal* 0.0001f;
            lightRayData.hitCollider = raycastHit2D.collider;
            lightRayData.hitCollider.gameObject.TryGetComponent(out LightUltility hitLightUltility);
             if (hitLightUltility != null)
             {
                 hitLightUltility.OnLightHit(lightRayData);
             }
        }
        else
        {
            Debug.Log("Light ray did not hit any collider.");
            lightRayData.hitpos = emitpoint + lightDir * 100f;
            lightRayData.hitCollider = null;
        }
        return lightRayData;
    }
}
