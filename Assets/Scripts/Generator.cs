using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [Range(1, 10)] public int resolution;

    public float xScale;

    public float yScale;

    public float heightScale;

    public float roughness;

    public bool obj;
    private DiamondSquare _diamondSquare;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("GENERATE");
            _diamondSquare = new DiamondSquare((int)(Math.Pow(2.0, resolution) + 1), xScale, yScale, heightScale, roughness);
            _diamondSquare.GenerateHeightmap();
        }
    }
}