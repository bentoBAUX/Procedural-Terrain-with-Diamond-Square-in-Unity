using System;
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Generate");
            GenerateTerrain();
        }
    }

    private void GenerateTerrain()
    {
        int size = (int)(Math.Pow(2, resolution) + 1);
        _diamondSquare = new DiamondSquare(size, xScale, yScale, heightScale, roughness, obj);
        float[,] heightmap = _diamondSquare.GetHeightMap();

    
    }
}
