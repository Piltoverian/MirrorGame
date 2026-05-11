using NUnit.Framework;
using UnityEngine;

public class LightRayData//use this to store the data of a light ray used to caculate the the merge of light ray and redraw later
{
    public GameObject hitObject;//use for graph logic
    public GameObject emitObject;//use for graph logic
    public float lightluminosity;//use for calculate the luminosity of the light ray
    public System.Collections.Generic.List<int> lightdiagramindex;//use for graph logic
    public Vector2 hitpos;//use for draw the light ray
    public Vector2 emitpos;//use for draw the light ray
    public Quaternion rayrot;//use for draw the light ray

    public LightRayData()
    {
        hitObject = null;
        emitObject = null;
        lightluminosity = 0f;
        lightdiagramindex = new System.Collections.Generic.List<int>();
        hitpos = Vector2.zero;
        emitpos = Vector2.zero;
        rayrot = Quaternion.identity;
    }   
}
