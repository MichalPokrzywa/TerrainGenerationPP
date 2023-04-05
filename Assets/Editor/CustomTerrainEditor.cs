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
    SerializedProperty perlinXProperty;
    SerializedProperty perlinYProperty;
    SerializedProperty perlinOffSetX;
    SerializedProperty perlinOffSetY;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    
    SerializedProperty perlinParameters;
    GUITableState perlinParameterTable;
    //fold outs--------------------------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;
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

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }

}
