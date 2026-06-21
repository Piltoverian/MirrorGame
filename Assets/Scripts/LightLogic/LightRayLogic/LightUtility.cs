using UnityEngine;

public abstract class LightUtility:MonoBehaviour
{
    public abstract void OnLightHit(LightRayData lightRayData);
    public abstract void OnLightGraphClear();
}
