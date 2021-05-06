using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{
    public CosmicAPI api;

    private void Start()
    {
        api = FindObjectOfType<CosmicAPI>();
    }
}