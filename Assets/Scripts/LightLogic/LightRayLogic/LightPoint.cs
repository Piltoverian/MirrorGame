using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LightRayData //use this to store the data of a light ray used to caculate the the merge of light ray and redraw later
{
    public Collider2D hitCollider;//use for graph logic
    public GameObject emitObject;//use for graph logic
    public float lightluminosity;//use for calculate the luminosity of the light ray
    public System.Collections.Generic.List<int> lightdiagramindex;//use for graph logic
    public Vector2 hitpos;//use for draw the light ray
    public Vector2 emitpos;//use for draw the light ray
    public Vector3 raydir;//use for draw the light ray
    public Color Color;//Color of the light ray
    
    // NEW FIELDS
    public float currentPercentage = 0f;
    public LightRay visualRay;

    public LightRayData()
    {
        hitCollider = null;
        emitObject = null;
        lightluminosity = 0f;
        lightdiagramindex = new System.Collections.Generic.List<int>();
        hitpos = Vector2.zero;
        emitpos = Vector2.zero;
        raydir = Vector3.zero;
        Color = Color.white;
        currentPercentage = 0f;
        visualRay = null;
    }   
}

public static class LightRayHelper
{
    public static LightRayData LightEmit(GameObject ultility, Vector3 lightDir, List<int> lightsourceindex, float LightLuminosity, Vector3 emitpoint, Color rayColor)
    {
        LightRayData lightRayData = new LightRayData();
        lightRayData.emitpos = emitpoint;
        lightRayData.emitObject = ultility.gameObject;
        lightRayData.lightdiagramindex.AddRange(lightsourceindex);
        lightRayData.lightluminosity = LightLuminosity;
        lightRayData.raydir = lightDir;
        lightRayData.Color = rayColor;
        
        Ray2D ray = new Ray2D(emitpoint, lightDir);
        RaycastHit2D raycastHit2D = Physics2D.Raycast(ray.origin, ray.direction, 100f, 1<<7);
        
        if (raycastHit2D)
        {
            lightRayData.hitpos = raycastHit2D.point + raycastHit2D.normal * 0.0001f;
            lightRayData.hitCollider = raycastHit2D.collider;
        }
        else
        {
            lightRayData.hitpos = emitpoint + lightDir * 100f;
            lightRayData.hitCollider = null;
        }

        // TẠO OBJECT TIA SÁNG & CHẠY COROUTINE
        if (LightVisualizer.Instance != null && LightVisualizer.Instance.GetLightRayPrefab() != null)
        {
            // Lưu vào graph nếu cần
            LightVisualizer.Instance.lightTotalGraph.Add(lightRayData);

            GameObject rayObj = LightVisualizer.Instance.GetRayFromPool();
            
            if (rayObj.TryGetComponent(out LineRenderer lr)) {
                lr.SetPositions(new Vector3[] { lightRayData.emitpos, lightRayData.hitpos });
            }
            if (rayObj.TryGetComponent(out LightRay lightRay)) {
                lightRay.SetColor(lightRayData.Color);
                lightRay.SetLuminosity(lightRayData.lightluminosity);
                
                lightRayData.visualRay = lightRay; 
                LightVisualizer.Instance.RegisterActiveRay();
                LightVisualizer.Instance.StartCoroutine(GrowAndHitRoutine(lightRayData, LightVisualizer.Instance.GetLightGrowTime()));
            }
        }
        return lightRayData;
    }

    public static LightRayData LightEmit(LightRayData lightRayData)
    {
        return LightEmit(lightRayData.emitObject, lightRayData.raydir, lightRayData.lightdiagramindex, lightRayData.lightluminosity, lightRayData.emitpos, lightRayData.Color);
    }

    private static System.Collections.IEnumerator GrowAndHitRoutine(LightRayData data, float growTime)
    {
        float timer = 0f;
        while (timer < growTime)
        {
            timer += Time.deltaTime;
            data.currentPercentage = Mathf.Clamp01(timer / growTime);
            if (data.visualRay != null) data.visualRay.SetCurrentPercentage(data.currentPercentage);
            yield return null;
        }
        
        data.currentPercentage = 1f;
        if (data.visualRay != null) data.visualRay.SetCurrentPercentage(1f);

        if (data.hitCollider != null && data.hitCollider.TryGetComponent(out LightUtility hitUtility))
        {
            hitUtility.OnLightHit(data);
        }

        if (LightVisualizer.Instance != null) {
            LightVisualizer.Instance.UnregisterActiveRay();
        }
    }
}
