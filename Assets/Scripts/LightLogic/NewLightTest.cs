using UnityEngine;

public class NewLightTest : MonoBehaviour
{
    bool started = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!started)
        {
            started = true;
            LightVisualizer.Instance.StartVisualize();
        }
    }
}
