using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovingMaterial : MonoBehaviour
{

    public float ScrollX = 1f;
    public float ScrollY = 1f;

    public Image image;

    void Update()
    {
        float OffsetX = Time.time * ScrollX;
        float OffsetY = Time.time * ScrollY;
        image.material.mainTextureOffset = new Vector2(OffsetX, OffsetY);
    }
}
