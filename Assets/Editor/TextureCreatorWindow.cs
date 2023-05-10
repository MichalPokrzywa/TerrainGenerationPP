using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Diagnostics;

public class TextureCreatorWindow : EditorWindow
{
    string filename = "ProceduralTexture";
    float perlinXScale;
    float perlinYScale;
    int perlinOctaves;
    float perlinPersistance;
    float perlinHeightScale;
    int perlinOffSetX ;
    int perlinOffSetY ;
    private bool alphaToggle = false;
    private bool seamlessToogle = false;
    private bool mapToggle = false;
    private Texture2D pTexture;

    private float brightness = 0.5f;
    private float contrast = 0.5f;

    [MenuItem("Window/Texture Create Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    void OnEnable()
    {
        pTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        filename = EditorGUILayout.TextField("Texture Name", filename);

        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);
        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0.0f, 0.1f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0.0f, 0.1f); ;
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10); ;
        perlinPersistance = EditorGUILayout.Slider("Persistance", perlinPersistance, 1, 10);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0, 1);
        perlinOffSetX = EditorGUILayout.IntSlider("Offset X", perlinOffSetX, 0, 10000);
        perlinOffSetY = EditorGUILayout.IntSlider("Offset Y", perlinOffSetY, 0, 10000);
        brightness = EditorGUILayout.Slider("Brightness", brightness, 0.0f, 2f);
        contrast = EditorGUILayout.Slider("Contrast", contrast, 0.0f, 2f); ;
        alphaToggle = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        seamlessToogle = EditorGUILayout.Toggle("Map?", seamlessToogle);
        mapToggle = EditorGUILayout.Toggle("Seamless", mapToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        float minColour = 1;
        float maxColour = 0;

        if(GUILayout.Button("Generate",GUILayout.Width(wSize)))
        {
            int w = 513;
            int h = 513;
            float pValue;
            Color pixCol = Color.white;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (seamlessToogle)
                    {
                        float u = (float)x / (float)w;
                        float v = (float)y / (float)h;

                        float noise00 = Utility.FractalBrownianMotion((x + perlinOffSetX) * perlinXScale,
                                                                    (y + perlinOffSetY) * perlinYScale,
                                                                    perlinOctaves,
                                                                    perlinPersistance) * perlinHeightScale;
                        float noise01 = Utility.FractalBrownianMotion((x + perlinOffSetX) * perlinXScale,
                                                                    (y + perlinOffSetY+h) * perlinYScale,
                                                                    perlinOctaves,
                                                                    perlinPersistance) * perlinHeightScale;
                        float noise10 = Utility.FractalBrownianMotion((x + perlinOffSetX+w) * perlinXScale,
                                                                    (y + perlinOffSetY) * perlinYScale,
                                                                    perlinOctaves,
                                                                    perlinPersistance) * perlinHeightScale;
                        float noise11 = Utility.FractalBrownianMotion((x + perlinOffSetX + w) * perlinXScale,
                                                                    (y + perlinOffSetY + h) * perlinYScale,
                                                                    perlinOctaves,
                                                                    perlinPersistance) * perlinHeightScale;
                        float noiseTotal = u * v * noise00 +
                                           u * (1 - v) * noise01 +
                                           (u - 1) * v * noise10 +
                                           (u - 1) * (v - 1) * noise11;

                        float value = (int)(256 * noiseTotal) + 50;
                        float r = Mathf.Clamp((int)noise00, 0, 255);
                        float b = Mathf.Clamp(value, 0, 255);
                        float g = Mathf.Clamp(value+50, 0, 255);
                        float a = Mathf.Clamp(value+100, 0, 255);

                        pValue = (r + g + b) / (3 * 255.0f);

                    }
                    else
                    {
                        pValue = Utility.FractalBrownianMotion((x + perlinOffSetX) * perlinXScale,
                                                        (y + perlinOffSetY) * perlinYScale,
                                                        perlinOctaves,
                                                        perlinPersistance) * perlinHeightScale;
                    }

                    float colValue = contrast * (pValue - 0.5f) + 0.5f * brightness;
                    if (minColour > colValue) 
                            minColour = colValue;
                    if (maxColour < colValue) 
                            maxColour = colValue;
                    pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    pTexture.SetPixel(x,y,pixCol);


                }
            }

            if (mapToggle)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        pixCol = pTexture.GetPixel(x, y);
                        float colValue = pixCol.r;
                        colValue = Utility.Map(colValue, minColour, maxColour, 0, 1);
                        pixCol.r = colValue;
                        pixCol.g = colValue;
                        pixCol.b = colValue;
                        pTexture.SetPixel(x,y,pixCol);
                    }
                }
            }
            pTexture.Apply(false,false);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(pTexture,GUILayout.Width(wSize),GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Width(wSize)))
        {
            byte[] bytes = pTexture.EncodeToPNG();
            System.IO.Directory.CreateDirectory(Application.dataPath + "/savedTextures");
            File.WriteAllBytes(Application.dataPath + "/savedTextures/" +filename+".png",bytes);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }

}
