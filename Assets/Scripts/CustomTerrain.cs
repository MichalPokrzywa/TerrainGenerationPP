using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    //Reset Terrain
    public bool resetTerrain = true;
    
    //Add Random Height
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);

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
    //Vonoroi-------------------------------------
    public int peakCount = 2;
    public float fallOff = 0.2f;
    public float dropOff = 0.6f;
    public float maxHeight = 0.5f;
    public float minHeight = 0.3f;
    public enum VoronoiType {Linear = 0, Power = 1, Combined = 2,PowerSin = 3}
    public VoronoiType voronoiType = VoronoiType.Linear;
    //MPD--------------------------------------------
    public float mpdRoughness = 2.0f;
    public float mpdMaxHeight = 2.0f;
    public float mpdMinHeight = -2.0f;
    public float mpdHeightDampener = 2.0f;
    public int mpdSmoothAmount = 1;
    //---------------------------------------------
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

    public void Vonoroi()
    {
        float[,] heightMap = GetHeightMap();
        for (int i = 0; i < peakCount; i++)
        {
            if (minHeight > maxHeight)
            {
                (minHeight, maxHeight) = (maxHeight, minHeight);
            }
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapResolution),
                                        UnityEngine.Random.Range(minHeight, maxHeight), 
                                        UnityEngine.Random.Range(0, terrainData.heightmapResolution));
            if(peak.y > heightMap[(int)peak.x, (int)peak.z])
                heightMap[(int)peak.x, (int)peak.z] = peak.y;

            Vector2 peakLocation = new Vector2(peak.x, peak.z);

            float maxDistance = Vector2.Distance(new Vector2(0, 0),
                new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));

            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                for (int y = 0; y < terrainData.heightmapResolution; y++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceTopeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float peakY = 0;

                        if(voronoiType == VoronoiType.Combined)
                            peakY = peak.y - distanceTopeak * fallOff - Mathf.Pow((distanceTopeak),dropOff);
                        else if(voronoiType == VoronoiType.Power)
                            peakY = peak.y - Mathf.Pow((distanceTopeak),dropOff) * fallOff;
                        else if(voronoiType == VoronoiType.Linear)
                            peakY = peak.y - distanceTopeak * fallOff;
                        else if (voronoiType == VoronoiType.PowerSin)
                            peakY = peak.y - Mathf.Pow(distanceTopeak * 3, fallOff) - 
                                    Mathf.Sin(distanceTopeak * 2 * Mathf.PI) / maxDistance;
                        if (heightMap[x,y] < peakY)
                             heightMap[x, y] = peakY;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MidPointDisplaceMent()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapResolution - 1;
        int squareSize = width;
        float heightMin = mpdMinHeight;
        float heightMax = mpdMaxHeight;
        float heightDampener = (float)Mathf.Pow(mpdHeightDampener, -1 * mpdRoughness); 

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        //heightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
        //heightMap[0, terrainData.heightmapResolution - 2] = UnityEngine.Random.Range(0f, 0.2f);
        //heightMap[terrainData.heightmapResolution - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
        //heightMap[terrainData.heightmapResolution - 2, terrainData.heightmapResolution - 2] =
        //    UnityEngine.Random.Range(0f, 0.2f);

        while(squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);
                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[cornerX, cornerY]) / 4.0f + 
                                                    UnityEngine.Random.Range(heightMin, heightMax));

                }
            }

            for (int x = 0; x < width; x+=squareSize)
            {
                for (int y = 0; y < width; y+=squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);
                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if(pmidXL <=0 || pmidYD <= 0 || pmidXR >= width-1 || pmidYU >= width -1)
                        continue;

                    //bottom side
                    heightMap[midX , y] = (float)((heightMap[midX, midY] +
                                                   heightMap[x, y] +
                                                   heightMap[midX, pmidYD] +
                                                   heightMap[cornerX, y]) / 4.0f +
                                                  UnityEngine.Random.Range(heightMin, heightMax));
                    //upper side
                    heightMap[midX, cornerY] = (float)((heightMap[midX, midY] +
                                                  heightMap[x, cornerY] +
                                                  heightMap[midX, pmidYU] +
                                                  heightMap[cornerX, cornerY]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));
                    //left side
                    heightMap[x, midY] = (float)((heightMap[midX, midY] +
                                                  heightMap[x, y] +
                                                  heightMap[pmidXL, midY] +
                                                  heightMap[cornerY, x]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));
                    //right side
                    heightMap[midX, y] = (float)((heightMap[midX, midY] +
                                                  heightMap[cornerX, y] +
                                                  heightMap[pmidXR, midY] +
                                                  heightMap[cornerX, cornerY]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            squareSize = (int)(squareSize / 2.0f);
            heightMax *= heightDampener;
            heightMin *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);

    }

    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain","Progress",smoothProgress);

        for (int i = 0; i < mpdSmoothAmount; i++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                for (int y = 0; y < terrainData.heightmapResolution; y++)
                {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y),
                        terrainData.heightmapResolution, terrainData.heightmapResolution);

                    foreach (Vector2 n in neighbours)
                    {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }
                    heightMap[x, y] = avgHeight / ((float) neighbours.Count + 1);
                }
            }

            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress/mpdSmoothAmount);
        }

        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x,0,width - 1),
                        Mathf.Clamp(pos.y + y, 0, height - 1));
                    if(!neighbours.Contains(nPos))
                        neighbours.Add(nPos);

                }
            }
        }
        return neighbours;
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
