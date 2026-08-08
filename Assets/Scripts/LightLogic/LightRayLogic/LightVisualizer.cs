using System;
using System.Collections.Generic;
using UnityEngine;

public class LightVisualizer : MonoBehaviour
{
    public static LightVisualizer Instance { get; private set; }

    [SerializeField] GameObject lightRayPrefab;
    [SerializeField] private float lightGrowTime = 1f;

    [Header("WaitingPhase")]
    [SerializeField] private float waitingPhaseTime = 1.5f;
    private bool isWaitingPhase = false;
    private float finishTime = 0f;

    public List<LightRayData> lightTotalGraph = new List<LightRayData>(); // Giữ lại mảng này cho Debug hoặc logic sau này
    
    // Mảng Object Pool
    private List<GameObject> lightActiveRayGameObjects = new List<GameObject>();
    private List<GameObject> lightRayGameObjectsPool = new List<GameObject>();
    
    private int activeCoroutines = 0; // Theo dõi xem còn tia sáng nào đang chạy không

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public GameObject GetLightRayPrefab() => lightRayPrefab;
    public float GetLightGrowTime() => lightGrowTime;

    // --- CẤP PHÁT & QUẢN LÝ TIA SÁNG (POOLING) ---
    public GameObject GetRayFromPool()
    {
        GameObject rayObj;
        if (lightRayGameObjectsPool.Count > 0)
        {
            rayObj = lightRayGameObjectsPool[0];
            lightRayGameObjectsPool.RemoveAt(0);
        }
        else
        {
            rayObj = Instantiate(lightRayPrefab);
        }
        rayObj.SetActive(true);
        lightActiveRayGameObjects.Add(rayObj);
        return rayObj;
    }

    // --- THEO DÕI TIẾN ĐỘ ĐỂ CHUẨN BỊ XÓA ---
    public void RegisterActiveRay() 
    {
        activeCoroutines++;
        isWaitingPhase = false; 
    }
    
    public void UnregisterActiveRay() 
    {
        activeCoroutines--;
        if (activeCoroutines <= 0) 
        {
            activeCoroutines = 0;
            // Đồ thị đã hoàn tất, bắt đầu đếm ngược thời gian chờ để xóa
            isWaitingPhase = true;
            finishTime = Time.time;
        }
    }

    private void Update() 
    {
        if (isWaitingPhase) 
        {
            if (Time.time >= finishTime + waitingPhaseTime) 
            {
                isWaitingPhase = false;
                LightGraphClear(); // Xóa toàn bộ đồ thị sau khi chờ xong
            }
        }
    }

    // --- KHỞI ĐỘNG VÀ XÓA ĐỒ THỊ ---
    public void StartVisualize()
    {
        LightGraphClear();
        var lightSources = FindObjectsByType<LightSource>();
        if (lightSources.Length == 0)
        {
            Debug.LogWarning("No LightSource found in the scene. Please add at least one LightSource to visualize light rays.");
            return;
        }
        foreach (var lightSource in lightSources)
        {
            var lightindexlist = new List<int> { lightSource.GetLightSourceIndex() };
            LightRayHelper.LightEmit(lightSource.gameObject, lightSource.transform.right, lightindexlist, lightSource.GetLightLuminosity(), lightSource.transform.position, lightSource.GetLightSourceColor());
        }
    }

    public void LightGraphClear()
    {
        lightTotalGraph.Clear();
        activeCoroutines = 0;
        isWaitingPhase = false;

        // Cất hết vào Pool
        foreach (var obj in lightActiveRayGameObjects)
        {
            obj.SetActive(false);
            lightRayGameObjectsPool.Add(obj);
        }
        lightActiveRayGameObjects.Clear();

        var lightUtilities = FindObjectsByType<LightUtility>();
        foreach (var ult in lightUtilities)
        {
            ult.OnLightGraphClear();
        }
    }
}
