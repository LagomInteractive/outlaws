using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{
    public CosmicAPI api;
    public bool destroy = false;

    private void Start()
    {
        api = FindObjectOfType<CosmicAPI>();
    }

    public void Update()
    {
        
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}