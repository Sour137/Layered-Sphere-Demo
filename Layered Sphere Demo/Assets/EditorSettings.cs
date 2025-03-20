using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Editor Settings", menuName = "Settings/World")]
public class EditorSettings : ScriptableObject
{

    public static int EXPANSION = 3;
    public static float BLENDPOSITION = 0.5f;
    public static string HOLDERNAME = "Generated Map";
    public static string DEBUGNAME = "Debug Object";

    public static int RANGE_OCEAN = 3700;
    public static int RANGE_SURFACE = 5000;
    public static int BELOW_DEEP = 1000;
    public static int BELOW_BATH = 200;
    public static int ABOVE_MEDIUM = 1000;
    public static int ABOVE_HIGH = 3000;

    public static int MINFUNK_LOWBOUND = 1;
    public static int MINFUNK_HIGBOUND = 4;
    public static int MINFUNK_FLOATSCALE = 1000000;

    public int worldSeed;
    public bool randomize;
    public bool debugPositions;
    public bool smootheNormals;
    [Range(4, 7)]
    public int nSize;
    [Range(0, 10)]
    public int smoothingPasses;
    [Range(0.25f, 0.99f)]
    public float percentOcean;
    [Range(20, 1500)]
    public int layers;
    [Range(100, 300)]
    public int terrainDelta;
    [Range(0.1f, 10f)]
    public float elevScale;
    [Range(1f, 100f)]
    public float radius;
    [Range(1f, 30f)]
    public float polarBuffer;
    [Range(-1f, 1f)]
    public float sealevelOffset;
    //[Range(1, 384)]
    //public int geoProvinceXScale;
    //[Range(1, 195)]
    //public int geoProvinceYScale;
    [Range(0.01f, 1f)]
    public float geoProvinceScale;
    [Range(0.05f, 1.0f)]
    public float geoProvinceThreshold;
    [Range(5000000f, 10000000f)]
    public float maxCratonArea;
    [Range(0.01f, 0.99f)]
    public float biogeographicScore;

    public int deltaFlat;
    public int deltaHill;

    public void Init()
    {
        deltaFlat = terrainDelta;
        deltaHill = deltaFlat + terrainDelta;
    }

}
