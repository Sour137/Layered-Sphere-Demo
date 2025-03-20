using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameSettings
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
    public int nSize;
    public int smoothingPasses;
    public float percentOcean;
    public int layers;
    public int terrainDelta;
    public float elevScale;
    public float radius;
    public float polarBuffer;
    public float sealevelOffset;
    //[Range(1, 384)]
    //public int geoProvinceXScale;
    //[Range(1, 195)]
    //public int geoProvinceYScale;
    public float geoProvinceScale;
    public float geoProvinceThreshold;
    public float maxCratonArea;
    public float biogeographicScore;

    public int deltaFlat;
    public int deltaHill;

    public GameSettings(EditorSettings in_settings)
    {
        worldSeed = in_settings.worldSeed;
        randomize = in_settings.randomize;
        debugPositions = in_settings.debugPositions;
        smootheNormals = in_settings.smootheNormals;
        nSize = in_settings.nSize;
        smoothingPasses = in_settings.smoothingPasses;
        percentOcean = in_settings.percentOcean;
        layers = in_settings.layers;
        terrainDelta = in_settings.terrainDelta;
        elevScale = in_settings.elevScale;
        radius = in_settings.radius;
        polarBuffer = in_settings.polarBuffer;
        sealevelOffset = in_settings.sealevelOffset;
        geoProvinceScale = in_settings.geoProvinceScale;
        geoProvinceThreshold = in_settings.geoProvinceThreshold;
        maxCratonArea = in_settings.maxCratonArea;
        biogeographicScore = in_settings.biogeographicScore;
        deltaFlat = in_settings.deltaFlat;
        deltaHill = in_settings.deltaHill;
    }

    public void Init()
    {
        deltaFlat = terrainDelta;
        deltaHill = deltaFlat + terrainDelta;
    }
}


