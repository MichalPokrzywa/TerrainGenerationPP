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
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXProperty;
    SerializedProperty perlinYProperty;
    SerializedProperty perlinOffSetX;
    SerializedProperty perlinOffSetY;
    //fold outs--------------------------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXProperty = serializedObject.FindProperty("perlinXScale");
        perlinYProperty = serializedObject.FindProperty("perlinYScale");
        perlinOffSetX = serializedObject.FindProperty("perlinOffSetX");
        perlinOffSetY = serializedObject.FindProperty("perlinOffSetY");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;
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
        showPerlin = EditorGUILayout.Foldout(showPerlin, "Perlin Noise");

        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Scale for Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXProperty,0,1,new GUIContent("X scale"));
            EditorGUILayout.Slider(perlinYProperty,0,1,new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffSetX, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffSetY, 0, 10000, new GUIContent("Y Offset"));
            if (GUILayout.Button("Generate Perlin"))
            {
                terrain.Perlin();
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
