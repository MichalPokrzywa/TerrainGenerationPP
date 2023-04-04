using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static float FractalBrownianMotion(float x, float y, int oct, float persistance, int offsetx, int offsety)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < oct; i++)
        {
            total += Mathf.PerlinNoise((x + offsetx) * frequency,(y + offsety) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= frequency;
            frequency *= 2;
        }

        return total/maxValue;
    }


}
