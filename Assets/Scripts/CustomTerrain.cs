using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    //Reset Terrain
    public bool resetTerrain = true;
    public List<float[,]> terrainPastDatas = new List<float[,]>();
    
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
    //Texture Layer---------------------------------------
    [System.Serializable]
    public class TextureLayer
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 1.5f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float textureOffset = 0.01f;
        public float noiseMultiplayer = 0.1f;
        public float noiseX = 0.01f;
        public float noiseY = 0.01f;
        public bool remove = false;
    }
    public List<TextureLayer> textureLayers = new List<TextureLayer>()
    {
        new TextureLayer()
    };
    //Vegetation-----------------------------------
    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public bool remove = false;
    }
    public List<Vegetation> vegetation = new List<Vegetation>()
    {
        new Vegetation()
    };

    public int maxTrees = 5000;
    public int treeSpacing = 5;
    //---------------------------------------------

    public Terrain terrain;
    public TerrainData terrainData;

    float[,] GetHeightMap()
    {
        terrainPastDatas.Add(terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution));
        return !resetTerrain ? terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution) : new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
    }

    public void UndoTerrain()
    {
        if (terrainPastDatas.Count > 0)
        {
            terrainData.SetHeights(0, 0, terrainPastDatas[terrainPastDatas.Count - 1]);
            terrainPastDatas.RemoveAt(terrainPastDatas.Count - 1);
        }
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

    public void AddNewTextureLayer()
    {
        textureLayers.Add(new TextureLayer());
    }

    public void RemoveTextureLayer()
    {
        List<TextureLayer> keptTextureLayers = new List<TextureLayer>();
        foreach (TextureLayer t in textureLayers)
        {
            if (!t.remove)
            {
                keptTextureLayers.Add(t);
            }
        }
        if (keptTextureLayers.Count == 0)
        {
            keptTextureLayers.Add(textureLayers[0]);
        }

        textureLayers = keptTextureLayers;
    }

    public void RemoveAllTextureFromTerrain()
    {
        terrainData.terrainLayers = null;
    }

    private float GetSteepness(float[,] heightMap, int x, int y, int width, int height)
    {
        float h= heightMap[x,y];
        int nx = x + 1;
        int ny = y + 1;

        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightMap[nx, y] - h;
        float dy = heightMap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);
        
        float steep = gradient.magnitude;

        return steep;
    }

    public void TextureLayers()
    {
        TerrainLayer[] newTerrainLayers = new TerrainLayer[textureLayers.Count];
        for (int tlIndex = 0; tlIndex < textureLayers.Count; tlIndex++)
        {
            TextureLayer texture = textureLayers[tlIndex];
            if (texture.texture != null)
            {
                newTerrainLayers[tlIndex] = new TerrainLayer();
                newTerrainLayers[tlIndex].diffuseTexture = texture.texture;
                newTerrainLayers[tlIndex].tileOffset = texture.tileOffset;
                newTerrainLayers[tlIndex].tileSize = texture.tileSize;
                newTerrainLayers[tlIndex].diffuseTexture.Apply(true);
            }
        }
        terrainData.terrainLayers = newTerrainLayers;

        float[,] heightMap =
            terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        float[,,] terrainLayerData =
            new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] layer = new float[terrainData.alphamapLayers];
                for (int i = 0; i < textureLayers.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x * textureLayers[i].noiseX, y * textureLayers[i].noiseY) * textureLayers[i].noiseMultiplayer;
                    float finalOffset = textureLayers[i].textureOffset + noise;
                    float thisHeightStart = textureLayers[i].minHeight - finalOffset;
                    float thisHeightStop = textureLayers[i].maxHeight + finalOffset;
                    //float steepness = GetSteepness(heightMap, x, y, terrainData.heightmapResolution,
                    //    terrainData.heightmapResolution);
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight,
                        x / (float)terrainData.alphamapWidth);
                    if (heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop &&
                        steepness >= textureLayers[i].minSlope && steepness <= textureLayers[i].maxSlope)
                    {
                        layer[i] = 1;
                    }
                }
                NormalizeVector(layer);
                for (int j = 0; j < textureLayers.Count; j++)
                {
                    terrainLayerData[x,y,j] = layer[j];
                }
            }
        }
        terrainData.SetAlphamaps(0,0,terrainLayerData);
    }

    private void NormalizeVector(float[] layer)
    {
        float total = 0;
        for (int i = 0; i < layer.Length; i++)
        {
            total += layer[i];
        }

        for (int i = 0; i < layer.Length; i++)
        {
            layer[i]/= total;
        }
    }

    public void AddNewVegetation()
    {
        vegetation.Add(new Vegetation());
    }

    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetations = new List<Vegetation>();
        foreach (Vegetation t in vegetation)
        {
            if (!t.remove)
            {
                keptVegetations.Add(t);
            }
        }
        if (keptVegetations.Count == 0)
        {
            keptVegetations.Add(vegetation[0]);
        }

        vegetation = keptVegetations;
    }

    public void PlantVegetation()
    {
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetation.Count];
        int index = 0;
        foreach (Vegetation t in vegetation)
        {
            newTreePrototypes[index] = new TreePrototype();
            newTreePrototypes[index].prefab = t.mesh;
            index++;
        }
        terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing)
        {
            for(int x = 0; x < terrainData.size.x; x += treeSpacing )
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float thisHeightStart = vegetation[tp].minHeight;
                    float thisHeightStop = vegetation[tp].maxHeight;
                    Debug.Log(thisHeight >= thisHeightStart && thisHeight <= thisHeightStop);
                    if (thisHeight >= thisHeightStart && thisHeight <= thisHeightStop)
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + Random.Range(-5.0f, 5.0f)) / terrainData.size.x,
                                terrainData.GetHeight(x, z) / terrainData.size.y, 
                                (z + Random.Range(-5.0f, 5.0f)) / terrainData.size.z);

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x,
                                                   instance.position.y * terrainData.size.y,
                                                   instance.position.z * terrainData.size.z) +
                                               this.transform.position;

                        int layerMask = 1 << terrainLayer;
                        RaycastHit hit;
                        //Debug.Log(Physics.Raycast(treeWorldPos, Vector3.down, out hit, 100, layerMask) ||
                        //          Physics.Raycast(treeWorldPos, Vector3.up, out hit, 100, layerMask)||
                        //          this.transform.position.y == instance.position.y);
                        if (Physics.Raycast(treeWorldPos + new Vector3(0, 10, 0), Vector3.down, out hit , 100, layerMask) || 
                            Physics.Raycast(treeWorldPos - new Vector3(0, 10, 0), Vector3.up, out hit, 100, layerMask))
                        {
                            float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                            instance.position = new Vector3(instance.position.x, treeHeight, instance.position.z);
                            instance.rotation = UnityEngine.Random.Range(0, 360);
                            instance.prototypeIndex = tp;
                            instance.color = Color.white;
                            instance.lightmapColor = Color.white;
                            instance.heightScale = 0.95f;
                            instance.widthScale = 0.95f;
                            allVegetation.Add(instance);
                            if (allVegetation.Count >= maxTrees)
                            {
                                terrainData.treeInstances = allVegetation.ToArray();
                                return;
                            }
                        }
                    }
                }
            }
        }
        terrainData.treeInstances = allVegetation.ToArray();
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
        for (int x = 0; x < terrainData.heightmapResolution-1; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution-1; y++)
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
        terrainPastDatas.Clear();
        terrainData.SetHeights(0, 0, heightMap);
    }

    void OnEnable()
    {
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }
    public enum TagType {Tag = 0, Layer =1}
    [SerializeField] private int terrainLayer = -1;
    void Awake()
    {
        SerializedObject tagManager = new SerializedObject
            (AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        AddTag(tagsProperty, "Terrain",TagType.Tag);
        AddTag(tagsProperty, "Cloud",TagType.Tag);
        AddTag(tagsProperty, "Shore", TagType.Tag);
        tagManager.ApplyModifiedProperties();

        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
    }

    int AddTag(SerializedProperty tagProperty, string newTag,TagType tType)
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

        if (!found && tType == TagType.Tag)
        {
            tagProperty.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProperty = tagProperty.GetArrayElementAtIndex(0);
            newTagProperty.stringValue = newTag;
        }
        else if (!found && tType == TagType.Layer)
        {
            for (int j = 8; j < tagProperty.arraySize; j++)
            {
                SerializedProperty newLayer = tagProperty.GetArrayElementAtIndex(j);

                if (newLayer.stringValue == "")
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }
        return -1;
    }

}
