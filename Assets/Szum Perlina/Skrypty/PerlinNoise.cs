using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;

public static class PerlinNoise
{
    // Hash lookup table as defined by Ken Perlin.  This is a randomly arranged array of all numbers from 0-255 inclusive.
    private static readonly int[] permutation = { 151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };
    private static readonly int[] p;
    static PerlinNoise()
    {
        p = new int[512];
        for (int x = 0; x < 512; x++)
        {
            p[x] = permutation[x % 256];
        }
    }
    static float lerp(float a, float b, float x)
    {
        return a + x * (b - a);
    }
    static float fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);  // 6t^5 - 15t^4 + 10t^3
    }

    #region Perlin 3D


    public static float Perlin3D(float x, float y, float z)
    {
        int unitX = (int)x & 255;
        int unitY = (int)y & 255;
        int unitZ = (int)z & 255;

        float xDecimalPart = x - (int)x;
        float yDecimalPart = y - (int)y;
        float zDecimalPart = z - (int)z;

        float u = fade(xDecimalPart);
        float v = fade(yDecimalPart);
        float w = fade(zDecimalPart);

        int aaa, aba, aab, abb, baa, bba, bab, bbb;
        aaa = p[p[p[unitX] + unitY] + unitZ];
        aba = p[p[p[unitX] + ++unitY] + unitZ];
        aab = p[p[p[unitX] + unitY] + ++unitZ];
        abb = p[p[p[unitX] + ++unitY] + ++unitZ];
        baa = p[p[p[++unitX] + unitY] + unitZ];
        bba = p[p[p[++unitX] + ++unitY] + unitZ];
        bab = p[p[p[++unitX] + unitY] + ++unitZ];
        bbb = p[p[p[++unitX] + ++unitY] + ++unitZ];

        float x1, x2, y1, y2;
        x1 = lerp(grad(aaa, xDecimalPart, yDecimalPart, zDecimalPart),           
                    grad(baa, xDecimalPart - 1, yDecimalPart, zDecimalPart),             
                    u);                                     
        x2 = lerp(grad(aba, xDecimalPart, yDecimalPart - 1, zDecimalPart),           
                    grad(bba, xDecimalPart - 1, yDecimalPart - 1, zDecimalPart),             
                      u);
        y1 = lerp(x1, x2, v);

        x1 = lerp(grad(aab, xDecimalPart, yDecimalPart, zDecimalPart - 1),
                    grad(bab, xDecimalPart - 1, yDecimalPart, zDecimalPart - 1),
                    u);
        x2 = lerp(grad(abb, xDecimalPart, yDecimalPart - 1, zDecimalPart - 1),
                      grad(bbb, xDecimalPart - 1, yDecimalPart - 1, zDecimalPart - 1),
                      u);
        y2 = lerp(x1, x2, v);

        return (lerp(y1, y2, w) + 1) / 2;
    }

    
    static float grad(int hash, float x, float y, float z)
    {
        switch (hash & 0xF)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            case 0x4: return x + z;
            case 0x5: return -x + z;
            case 0x6: return x - z;
            case 0x7: return -x - z;
            case 0x8: return y + z;
            case 0x9: return -y + z;
            case 0xA: return y - z;
            case 0xB: return -y - z;
            case 0xC: return y + x;
            case 0xD: return -y + z;
            case 0xE: return y - x;
            case 0xF: return -y - z;
            default: return 0; // never happens
        }
    }
    #endregion

    #region Perlin 2D

    static private Vector2[] gradients = new Vector2[]
    {
        new Vector2(0,1), new Vector2(1,1), new Vector2(1,0), new Vector2(1,-1),
        new Vector2(0,-1), new Vector2(-1,-1), new Vector2(-1,0),new Vector2(-1,1)
    };
    static float dot2D(int i, float x, float y)
    {
        return
            gradients[i].x * x + gradients[i].y * y;
    }

    public static float Perlin2D(float x, float y)
    {
        int ix = (int)x;
        int iy = (int)y;

        x = x - ix;
        y = y - iy;

        float fx = fade(x);
        float fy = fade(y);

        ix &= 255;
        iy &= 255;

        // here is where i get the index to look up in the list of 
        // different gradients.
        // p is my array of 0-255 in random order
        int g00 = p[iy + p[ix]] & 7;
        int g10 = p[iy + p[ix + 1]] & 7;
        int g01 = p[iy + 1 + p[ix]] & 7;
        int g11 = p[iy + 1 + p[ix + 1]] & 7;

        // this takes the dot product to find the values to interpolate between
        float n00 = dot2D(g00 , x, y);
        float n10 = dot2D(g10, x - 1, y);
        float n01 = dot2D(g01, x, y - 1);
        float n11 = dot2D(g11, x - 1, y - 1);

        // lerp() is just normal linear interpolation
        float y1 = lerp(n00, n10, fx);
        float y2 = lerp(n01, n11, fx);

        return
            lerp(y1, y2, fy);
    }

    #endregion
}