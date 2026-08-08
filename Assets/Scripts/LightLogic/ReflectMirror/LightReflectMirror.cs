using System.Collections.Generic;
using UnityEngine;

public class LightReflectMirror : LightUtility
{
    private struct EmittedRay
    {
        public Vector2 position;
        public Vector3 direction;
        public LightRayData outgoingData;
        public Color incomingColor;
    }

    private List<EmittedRay> currentEmittedRays = new List<EmittedRay>();

    [Header("Anti-Loop Settings")]
    [SerializeField] private float angleTolerance = 1f;
    [SerializeField] private float posTolerance = 0.1f;
    [SerializeField] private int maxBounces = 5; // Có thể giữ lại nhưng cách map dưới đây đã chống lặp khá tốt

    public override void OnLightHit(LightRayData lightRayData)
    {
        if (Vector3.Dot(lightRayData.raydir, transform.up) > 0.01f)
        {
            return;
        }

        Vector3 reflectDir = CalculateReflectDir(lightRayData);
        Vector2 reflectPos = lightRayData.hitpos;

        // KIỂM TRA TIA ĐÃ PHẢN XẠ CHƯA
        for (int i = 0; i < currentEmittedRays.Count; i++)
        {
            float posDiff = Vector2.Distance(currentEmittedRays[i].position, reflectPos);
            float angleDiff = Vector3.Angle(currentEmittedRays[i].direction, reflectDir);

            if (posDiff <= posTolerance && angleDiff <= angleTolerance)
            {
                // Tia này đã từng được phản xạ! Giờ chỉ việc truyền màu mới (Instant Color Update)
                EmittedRay emitted = currentEmittedRays[i];
                if (emitted.incomingColor != lightRayData.Color)
                {
                    emitted.incomingColor = lightRayData.Color;
                    emitted.outgoingData.Color = lightRayData.Color;
                    
                    if (emitted.outgoingData.visualRay != null)
                    {
                        emitted.outgoingData.visualRay.SetColor(lightRayData.Color);
                    }
                    currentEmittedRays[i] = emitted; // Cập nhật lại mảng

                    // Gõ cửa đầu bên kia bắt đổi màu theo
                    if (emitted.outgoingData.hitCollider != null && emitted.outgoingData.hitCollider.TryGetComponent(out LightUtility nextTarget))
                    {
                        nextTarget.OnLightHit(emitted.outgoingData);
                    }
                }
                return; // Thoát luôn, không mọc tia mới!
            }
        }

        // TIA CHƯA PHẢN XẠ BAO GIỜ -> MỌC TIA MỚI
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        int[] oldLayers = new int[allChildren.Length];
        for (int i = 0; i < allChildren.Length; i++)
        {
            oldLayers[i] = allChildren[i].gameObject.layer;
            allChildren[i].gameObject.layer = 2; 
        }

        var lightdata = LightRayHelper.LightEmit(gameObject, reflectDir, lightRayData.lightdiagramindex, lightRayData.lightluminosity, lightRayData.hitpos, lightRayData.Color);

        for (int i = 0; i < allChildren.Length; i++)
        {
            allChildren[i].gameObject.layer = oldLayers[i];
        }

        if (lightdata != null)
        {
            // Lưu lại để lần sau chỉ update màu
            currentEmittedRays.Add(new EmittedRay { position = reflectPos, direction = reflectDir, outgoingData = lightdata, incomingColor = lightRayData.Color });
        }
    }

    public Vector3 CalculateReflectDir(LightRayData incomingRay)
    {
        if (incomingRay.hitCollider == null)
        {
            return Vector3.zero;
        }
        Vector3 normal = transform.up;
        return incomingRay.raydir - 2 * Vector3.Dot(incomingRay.raydir, normal) * normal;
    }

    public override void OnLightGraphClear()
    {
        currentEmittedRays.Clear();
    }
}
