using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;


public class WorldGrid
{
    // vars
    const int LIM_L = 0;
    const int LIM_H = 14;
    const int LIM_WINDOWH = 90;
    const int LIM_WINDOWW = 200;
    const int MAX_ELEVATION = 64;
    //float[,] techBarrier;
    List<River> rivers;
    //List<MineralRegion> mineralRegions;
    //WaterCell[] waterBodies;
    //LandCell[] landBodies;
    WorldTileArea wtaFunk;
    int[] elevationCounts;
    int[] altitudes;
    float highest;
    int largestMineralRegion;
    int longestRiver;
    //float largestWaterBody;
    //float largestLandMass;
    float largestTechBody;
    int oceanPos;
    float wettest;
    char[] elevationTypes;
    float elevScale;
    float[] actualLayers;
    MinMax layerElevMinMax;
    float oceanRadius;
    float[] actualRadii;

    // Province to WorldNode Refactor
    WorldNode[,] nodes;
    //WorldNodeType types;

    /// <summary>
    /// The useful constructor. World will be 2^in_n + 1 tall, and 2(2^in_n) wide
    /// </summary>
    /// <param name="in_n">The determinate dimension. There is not enough memory for values greater than 13, so it is checked</param>
    public WorldGrid(int in_n, Random in_rando)
    {
        int y = Calc_maxY(in_n);
        int x = Calc_maxX(y);

        wtaFunk = new WorldTileArea(x, y);
        rivers = new List<River>();
        longestRiver = 0;
        //mineralRegions = new List<MineralRegion>();
        largestMineralRegion = 0;
        //waterBodies = new WaterCell[0];
        //largestWaterBody = 0;
        //largestLandMass = 0;
        elevationCounts = new int[(int)(MAX_ELEVATION * 1.25)];
        nodes = InitializeNodes(x, y);
    }

    private WorldNode[,] InitializeNodes(int in_maxX, int in_maxY)
    {
        int x = 0;
        int y = 0;
        WorldNode[,] toRet = new WorldNode[in_maxX, in_maxY];
        while (x < in_maxX)
        {
            while (y < in_maxY)
            {
                toRet[x, y] = new WorldNode();
                y++;
            }
            y = 0;
            x++;
        }
        return toRet;
    }

    //// Setters, and accessors
    public void Set_nodes(WorldNode[,] in_nodes) { nodes = in_nodes; }
    //public void Set_types(WorldNodeType in_types) { types = in_types; }
    //public void Set_techBarrier(float[,] in_map) { techBarrier = in_map; }
    public void Set_rivers(List<River> in_list) { rivers = in_list; }
    //public void Set_mineralRegions(List<MineralRegion> in_list) { mineralRegions = in_list; }
    //public void Set_waterBodies(WaterCell[] in_set) { waterBodies = in_set; }
    //public void Set_landBodies(LandCell[] in_set) { landBodies = in_set; }
    public void Set_highest(int in_highest) { highest = in_highest; }
    public void Set_largestMineralRegion(int in_lrgMinReg) { largestMineralRegion = in_lrgMinReg; }
    public void Set_longestRiver(int in_length) { longestRiver = in_length; }
    //public void Set_largestWaterBody(float in_size) { largestWaterBody = in_size; }
    //public void Set_largestLandMass(float in_size) { largestLandMass = in_size; }
    public void Set_largestTechBody(float in_size) { largestTechBody = in_size; }
    public void Set_oceanPos(int in_oceanElev) { oceanPos = in_oceanElev; }
    public void Set_wettest(float in_precip) { wettest = in_precip; }
    public void Add_elevationCounts(int in_elevation) { elevationCounts[in_elevation]++; }
    public void Set_elevationCounts(int[] in_set) { elevationCounts = in_set; }
    public void Set_elevationTypes(char[] in_set) { elevationTypes = in_set; }
    public void Set_altitudes(int[] in_set) { altitudes = in_set; }
    public void Set_elevScale(float ins) { elevScale = ins; }
    public void Set_actualLayers(float[] in_actualLayers) { actualLayers = in_actualLayers; }
    public void Set_layerElevMinMax(MinMax ins) { layerElevMinMax = ins; }
    public void Set_oceanRadius(float ins) { oceanRadius = ins; }
    public void Set_actualRadii(float[] ins) { actualRadii = ins; }


    public int Get_MaxElevation() { return MAX_ELEVATION; }
    public float Get_highest() { return highest; }
    public int Get_largestMineralRegion() { return largestMineralRegion; }
    public int Get_longestRiver() { return longestRiver; }
    //public float Get_largestWaterBody() { return largestWaterBody; }
    //public float Get_largestLandMass() { return largestLandMass; }
    public float Get_largestTechBody() { return largestTechBody; }
    public WorldNode Get_node(int in_x, int in_y) { return nodes[in_x, in_y]; }
    public WorldNode Get_node(Point2D in_point) { return nodes[in_point.x, in_point.y]; }
    public WorldNode[,] Get_Nodes() { return nodes; }
    //public WorldNodeType Get_types() { return types; }
    //public float[,] Get_techBarrier() { return techBarrier; }
    public List<River> Get_rivers() { return rivers; }
    //public List<MineralRegion> Get_mineralRegions() { return mineralRegions; }
    //public WaterCell[] Get_waterBodies() { return waterBodies; }
    //public LandCell[] Get_landBodies() { return landBodies; }
    public float Get_wettest() { return wettest; }
    public int[] Get_elevationCounts() { return elevationCounts; }
    public char[] Get_elevationTypes() { return elevationTypes; }
    public int[] Get_altitudes() { return altitudes; }
    public int Get_oceanPos() { return oceanPos; }
    public WorldTileArea Get_wtaFunk() { return wtaFunk; }
    public float Get_elevScale() { return elevScale; }
    public float[] Get_actualLayers() { return actualLayers; }
    public MinMax Get_layerElevMinMax() { return layerElevMinMax; }
    public float Get_oceanRadius() { return oceanRadius; }
    public float[] Get_actualradii() { return actualRadii; }
    //public WorldNodeType Get_nodeTypes() { return types; }

    /// <summary>
    /// This returns the x dimension which is 2(2^n).
    /// </summary>
    /// <param name="in_y">in_y = 2^n</param>
    /// <returns>2(in_y)</returns>
    private int Calc_maxX(int in_y)
    {
        int x = 2 * (in_y - 1);
        return x;
    }
    /// <summary>
    /// This returns the y dimension, which is 2^n + 1
    /// </summary>
    /// <param name="in_n">An integer between 0 and 13</param>
    /// <returns>2^in_n + 1</returns>
    private int Calc_maxY(int in_n)
    {
        int y = (int)Math.Pow(2, in_n) + 1;
        return y;
    }
}

