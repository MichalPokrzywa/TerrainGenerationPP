using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{
    //variables--------------------------
    SerializedProperty resetTerrain;
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    //perlin
    SerializedProperty perlinXProperty;
    SerializedProperty perlinYProperty;
    SerializedProperty perlinOffSetX;
    SerializedProperty perlinOffSetY;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    SerializedProperty perlinParameters;
    GUITableState perlinParameterTable;
    //vonoroi
    SerializedProperty peakCount;
    SerializedProperty fallOff;
    SerializedProperty dropOff;
    SerializedProperty maxHeight;
    SerializedProperty minHeight;
    SerializedProperty voronoiType;
    //mpd
    SerializedProperty mpdMinHeight;
    SerializedProperty mpdMaxHeight;
    SerializedProperty mpdHeightDampener;
    SerializedProperty mpdRoughness;
    SerializedProperty mpdSmoothAmount;
    //fold outs--------------------------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;
    bool showVonoroi = false;
    bool showMPD = false;
    bool showSmooth = false;
    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXProperty = serializedObject.FindProperty("perlinXScale");
        perlinYProperty = serializedObject.FindProperty("perlinYScale");
        perlinOffSetX = serializedObject.FindProperty("perlinOffSetX");
        perlinOffSetY = serializedObject.FindProperty("perlinOffSetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
        perlinParameterTable = new GUITableState("perlinParameterTable");
        peakCount = serializedObject.FindProperty("peakCount");
        fallOff = serializedObject.FindProperty("fallOff");
        dropOff = serializedObject.FindProperty("dropOff");
        maxHeight = serializedObject.FindProperty("maxHeight");
        minHeight = serializedObject.FindProperty("minHeight");
        voronoiType = serializedObject.FindProperty("voronoiType");
        mpdMinHeight = serializedObject.FindProperty("mpdMinHeight");
        mpdMaxHeight = serializedObject.FindProperty("mpdMaxHeight");
        mpdHeightDampener = serializedObject.FindProperty("mpdHeightDampener");
        mpdRoughness = serializedObject.FindProperty("mpdRoughness");
        mpdSmoothAmount = serializedObject.FindProperty("mpdSmoothAmount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;
        EditorGUILayout.PropertyField(resetTerrain);
        
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
            GUILayout.Label("Set Height Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Height Map");
       
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }
        showPerlin = EditorGUILayout.Foldout(showPerlin, "Single Perlin Noise");

        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Scale for Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXProperty,0,1,new GUIContent("X scale"));
            EditorGUILayout.Slider(perlinYProperty,0,1,new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffSetX, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffSetY, 0, 10000, new GUIContent("Y Offset"));
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));
            if (GUILayout.Button("Generate Perlin"))
            {
                terrain.Perlin();
            }
        }

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
        if (showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParameterTable =
                GUITableLayout.DrawTable(perlinParameterTable, serializedObject.FindProperty("perlinParameters"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply"))
            {
                terrain.MultiplePerlinTerrain();
            }
        }
        showVonoroi = EditorGUILayout.Foldout(showVonoroi, "Voronoi");
        if (showVonoroi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.IntSlider(peakCount, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(fallOff, 0, 10, new GUIContent("Fall off"));
            EditorGUILayout.Slider(dropOff, 0, 10, new GUIContent("Drop off"));
            EditorGUILayout.Slider(minHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(maxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);
            if (GUILayout.Button("Voronoi"))
            {
                terrain.Vonoroi();
            }

        }
        showMPD = EditorGUILayout.Foldout(showMPD, "MPD");

        if (showMPD)
        {
            EditorGUILayout.PropertyField(mpdMaxHeight);
            EditorGUILayout.PropertyField(mpdMinHeight);
            EditorGUILayout.PropertyField(mpdHeightDampener);
            EditorGUILayout.PropertyField(mpdRoughness);
            if (GUILayout.Button("MidPoint Displacement"))
            {
                terrain.MidPointDisplaceMent();
            }
        }
        showSmooth = EditorGUILayout.Foldout(showSmooth, "Smooth");

        if (showSmooth)
        {
            EditorGUILayout.IntSlider(mpdSmoothAmount, 1, 10, new GUIContent("Smooth Amount"));
            if (GUILayout.Button("Smooth"))
            {
                terrain.Smooth();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }

}
