using System;
using UnityEditor;
using UnityEngine;

public class Generator : EditorWindow
{
    private int _resolution = 5; // Must be 2^n + 1
    private float _xScale = 1000f;
    private float _yScale = 1000f;
    private float _heightScale = 100f;
    private float _roughness = 1f;

    [MenuItem("Tools/Diamond Square Generator")]
    public static void ShowWindow()
    {
        Generator window = CreateWindow<Generator>();
        window.titleContent = new GUIContent("Diamond Square Generator");
        window.ShowUtility();
    }

    private void OnGUI()
    {
        GUILayout.Label("Diamond Square Parameters", EditorStyles.boldLabel);

        _resolution = EditorGUILayout.IntSlider("Resolution", _resolution, 1, 10);
        _xScale = EditorGUILayout.FloatField("X Scale", _xScale);
        _yScale = EditorGUILayout.FloatField("Y Scale", _yScale);
        _heightScale = EditorGUILayout.FloatField("Height Scale", _heightScale);
        _roughness = EditorGUILayout.FloatField("Roughness", _roughness);

        if (GUILayout.Button("Generate and Export"))
        {
            GenerateAndExport();
        }
    }

    private void GenerateAndExport()
    {
        int size = (int)(Math.Pow(2, _resolution) + 1);
        if (size < 2 || (size - 1 & (size - 2)) != 0)
        {
            Debug.LogError("Size must be 2^n + 1 (e.g., 129, 257, etc.)");
            return;
        }

        string savePath = EditorUtility.SaveFilePanel("Save OBJ File", "", "DiamondSquare.obj", "obj");
        if (!string.IsNullOrEmpty(savePath))
        {
            // Call the DiamondSquare class
            DiamondSquare diamondSquare = new DiamondSquare(size, _xScale, _yScale, _heightScale, _roughness, true);
            diamondSquare.GenerateHeightmap(savePath);
            Debug.Log($"Diamond Square OBJ exported to: {savePath}");
        }
        else
        {
            Debug.LogWarning("Export cancelled.");
        }
    }
}
