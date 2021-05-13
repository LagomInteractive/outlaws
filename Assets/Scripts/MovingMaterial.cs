using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingMaterial : MonoBehaviour
{

    public float ScrollX = 1f;
    public float ScrollY = 1f;

    void Update()
    {
        float OffsetX = Time.time * ScrollX;
        float OffsetY = Time.time * ScrollY;
        GetComponent<Renderer>().material.SetTextureOffset("_EmissionTex", new Vector2(OffsetX, OffsetY));
    }
}
