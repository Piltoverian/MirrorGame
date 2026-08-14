using System.Collections.Generic;
using UnityEngine;

public class LightRendererPipeLine : MonoBehaviour
{
    [Header("Ray Casting")]
    [SerializeField] private GameObject lightRayPrefab;
    [SerializeField] private LayerMask lightCollisionMask = (1 << 6) | (1 << 7) | (1 << 8);
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Travel Rendering")]
    [SerializeField] private bool animateTravelTime = true;
    [SerializeField] private bool restartTravelWhenGraphChanges = true;
    [SerializeField] private float defaultLightSpeed = 20f;
    [SerializeField] private float baseRayWidth = 0.08f;
    [SerializeField] private float widthPerLuminosity = 0.01f;
    [SerializeField] private float maxRayWidth = 0.35f;

    [SerializeField]List<LightRayData> lightTotalGraph = new List<LightRayData>();

    private List<GameObject> lightActiveRayGameObjects = new List<GameObject>();
    private List<GameObject> lightRayGameObjectsPool = new List<GameObject>();
    private float travelClockStartTime;
    private int lastGraphSignature;

    private void OnEnable()
    {
        travelClockStartTime = Time.time;
        lastGraphSignature = 0;
    }

    public List<LightRayData> LightCalculatePhase()
    {
        lightTotalGraph.Clear();

        LightRayHelper.LightCollisionMask = lightCollisionMask.value;
        LightRayHelper.MaxDistance = maxRayDistance;
        LightRayHelper.DefaultLightSpeed = defaultLightSpeed;

        LightSource[] lightSources = FindObjectsByType<LightSource>();

        foreach (var lightSource in lightSources)
        {
            var lightindexlist= new List<int>();
            lightindexlist.Add(lightSource.GetLightSourceIndex());
            var lightRayData = LightRayHelper.LightEmit(
                lightSource.gameObject,
                lightSource.transform.right,
                lightindexlist,
                lightSource.GetLightLuminosity(),
                lightSource.transform.position,
                lightSource.GetLightColor(),
                lightSource.GetLightSpeed(defaultLightSpeed),
                0f
            );
            lightTotalGraph.Add(lightRayData);
        }
        return lightTotalGraph;
    }

    public void LightRenderPhase(List<LightRayData> lightRayDataList)
    {
        foreach (var lightRayData in lightRayDataList)
        {
            if (lightRayPrefab == null)
            {
                Debug.LogError("LightRayPrefab is not assigned.");
                return;
            }

            GameObject lightRayGameObject;
            if (lightRayGameObjectsPool.Count > 0)
            {
                lightRayGameObject = lightRayGameObjectsPool[0];
                lightRayGameObjectsPool.RemoveAt(0);
                lightRayGameObject.SetActive(true);
            }
            else
            {
                lightRayGameObject = Instantiate(lightRayPrefab);
            }
            if (lightRayGameObject.TryGetComponent(out LineRenderer lineRenderer))
            {
                Vector3 visibleEnd = GetVisibleEndPoint(lightRayData);

                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(new Vector3[] { lightRayData.emitpos, visibleEnd });

                Color rayColor = LightColorHelper.ToUnityColor(lightRayData.lightColor);
                lineRenderer.startColor = rayColor;
                lineRenderer.endColor = rayColor;

                float width = Mathf.Clamp(
                    baseRayWidth + lightRayData.lightluminosity * widthPerLuminosity,
                    baseRayWidth,
                    maxRayWidth
                );
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;

                lightActiveRayGameObjects.Add(lightRayGameObject);
            }
            else
            {
                Debug.LogError("LightRayPrefab does not have a LineRenderer component.");
            }
        }
    }

    public void AddAnEdgeToLightGraph(LightRayData lightRayData)
    {
        lightTotalGraph.Add(lightRayData);
    }

    public void UpdateLightGraph(LightRayData lightRayData)
    {
        for (int i = 0; i < lightTotalGraph.Count; i++)
        {
            var raydata = lightTotalGraph[i];
            if (lightRayData.emitObject == raydata.emitObject && lightRayData.emitpos == raydata.emitpos && lightRayData.raydir == raydata.raydir)
            {
                lightTotalGraph[i] = lightRayData;
                return;
            }
        }
        AddAnEdgeToLightGraph(lightRayData);
    }

    public LightRayData GetLightRayData(GameObject gameObject, Vector2 emitpos,Vector3 raydir)
    {
        foreach (var lightRayData in lightTotalGraph)
        {
            if (lightRayData.emitObject == gameObject && lightRayData.emitpos == emitpos && lightRayData.raydir == raydir)
            {
                return lightRayData;
            }
        }
        return null; // Return null if no matching light ray data is found
    }

    private void LightGraphClear()
    {
        LightUtility[] lightUtilities = FindObjectsByType<LightUtility>();
        for(int i = 0; i < lightUtilities.Length; i++)
        {
            lightUtilities[i].OnLightGraphClear();
        }

        for (int i = 0; i < lightActiveRayGameObjects.Count; i++)
        {
            if (lightActiveRayGameObjects[i] == null) continue;
            lightActiveRayGameObjects[i].SetActive(false);
            lightRayGameObjectsPool.Add(lightActiveRayGameObjects[i]);
        }

        lightActiveRayGameObjects.Clear();
        lightTotalGraph.Clear();
    }

    public void Update()
    {
        LightGraphClear();
        LightCalculatePhase();
    }

    public void LateUpdate()
    {
        UpdateTravelClockIfNeeded();
        LightRenderPhase(lightTotalGraph);
    }

    public void ResetLightTravelClock()
    {
        travelClockStartTime = Time.time;
        lastGraphSignature = 0;
    }

    private Vector3 GetVisibleEndPoint(LightRayData rayData)
    {
        if (!animateTravelTime || rayData.segmentTravelTime <= 0f)
        {
            return rayData.hitpos;
        }

        float elapsed = Time.time - travelClockStartTime - rayData.pathDelay;
        float visibleDistance = Mathf.Clamp(elapsed * rayData.lightSpeed, 0f, rayData.distance);
        return rayData.emitpos + (Vector2)(rayData.raydir.normalized * visibleDistance);
    }

    private void UpdateTravelClockIfNeeded()
    {
        if (!restartTravelWhenGraphChanges)
        {
            return;
        }

        int signature = CalculateGraphSignature(lightTotalGraph);
        if (lastGraphSignature != 0 && signature != lastGraphSignature)
        {
            travelClockStartTime = Time.time;
        }

        lastGraphSignature = signature;
    }

    private int CalculateGraphSignature(List<LightRayData> rays)
    {
        unchecked
        {
            int hash = 17;

            for (int i = 0; i < rays.Count; i++)
            {
                LightRayData ray = rays[i];
                if (ray == null) continue;

                hash = hash * 31 + (ray.emitObject != null ? ray.emitObject.GetInstanceID() : 0);
                hash = hash * 31 + (ray.hitCollider != null ? ray.hitCollider.GetInstanceID() : 0);
                hash = hash * 31 + Mathf.RoundToInt(ray.emitpos.x * 100f);
                hash = hash * 31 + Mathf.RoundToInt(ray.emitpos.y * 100f);
                hash = hash * 31 + Mathf.RoundToInt(ray.hitpos.x * 100f);
                hash = hash * 31 + Mathf.RoundToInt(ray.hitpos.y * 100f);
                hash = hash * 31 + Mathf.RoundToInt(ray.raydir.x * 100f);
                hash = hash * 31 + Mathf.RoundToInt(ray.raydir.y * 100f);
                hash = hash * 31 + (int)ray.lightColor;
                hash = hash * 31 + Mathf.RoundToInt(ray.lightluminosity * 100f);
            }

            return hash;
        }
    }
}
