using UnityEngine;

public class LightTest : MonoBehaviour
{
   public void Start()
   {
       var lightRendererPipeLine = FindAnyObjectByType<LightRendererPipeLine>();
       var lightRayDataList = lightRendererPipeLine.LightCalculatePhase();
       lightRendererPipeLine.LightRenderPhase(lightRayDataList);
   }

    private void OnDrawGizmos()
    {
        var lightRendererPipeLine = FindAnyObjectByType<LightRendererPipeLine>();
        var lightRayDataList = lightRendererPipeLine.LightCalculatePhase();
        lightRendererPipeLine.LightRenderPhase(lightRayDataList);
    }
}
