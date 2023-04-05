using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

    public bool resetTerrain = true;
    //Perlin Noise-----------------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffSetX = 0;
    public int perlinOffSetY  = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;

    //Mutiple Perlin----------------------------
    [System.Serializable]
    public class PerlinParameters
    {
        public float perlinXScale = 0.01f;
        public float perlinYScale = 0.01f;
        public int perlinOctaves = 3;
        public float perlinPersistance = 8;
        public float perlinHeightScale = 0.09f;
        public int perlinOffSetX = 0;
        public int perlinOffSetY = 0;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };

    public Terrain terrain;
    public TerrainData terrainData;

    float[,] GetHeightMap()
    {
        return !resetTerrain ? terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution) : new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] += Utility.FractalBrownianMotion((x+perlinOffSetX) * perlinXScale,
                                                                (y+perlinOffSetY) * perlinYScale,
                                                                perlinOctaves,
                                                                perlinPersistance) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utility.FractalBrownianMotion((x + p.perlinOffSetX) * p.perlinXScale,
                                                                    (y + p.perlinOffSetY) * p.perlinYScale,
                                                                    p.perlinOctaves,
                                                                    p.perlinPersistance) * p.perlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParametersList = new List<PerlinParameters>();
        foreach (var t in perlinParameters)
        {
            if (!t.remove)
            {
                keptPerlinParametersList.Add(t);
            }
        }

        if (keptPerlinParametersList.Count == 0)
        {
            keptPerlinParametersList.Add(perlinParameters[0]);
        }

        perlinParameters = keptPerlinParametersList;
    }
    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();
        //heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0,0,heightMap);
    }

    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] += heightMapImage.GetPixel((int)(x * heightMapScale.x),(int)(y* heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetTerrain()
    {
        float[,] heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        //heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    void OnEnable()
    {
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    void Awake()
    {
        SerializedObject tagManager = new SerializedObject
            (AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        AddTag(tagsProperty, "Terrain");
        AddTag(tagsProperty, "Cloud");
        AddTag(tagsProperty, "Shore");


        tagManager.ApplyModifiedProperties();
        this.gameObject.tag = "Terrain";
    }

    void AddTag(SerializedProperty tagProperty, string newTag)
    {
        bool found = false;

        for (int i = 0; i < tagProperty.arraySize; i++)
        {
            SerializedProperty tag = tagProperty.GetArrayElementAtIndex(i);
            if (tag.stringValue.Equals(newTag))
            {
                found = true; break;
            }
        }

        if (!found)
        {
            tagProperty.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProperty = tagProperty.GetArrayElementAtIndex(0);
            newTagProperty.stringValue = newTag;
        }

    }

}
