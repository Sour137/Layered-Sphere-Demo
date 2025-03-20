using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;


public class World
{
    WorldGrid grid;
    WorldNodeType types;
    //BiomeTable biomes;
    BiogeographicAmalgamator biogeographicZones;
    WorldAnalyzer analyzer;
    WaterAmalgamateRevTwo waterBodies;
    LandAmalgamate landBodies;
    RiverSystem rivers;


    /// <summary>
    /// Simple setter for the tile grid.
    /// </summary>
    /// <param name="in_grid">The inbound tile grid.</param>
    public void Set_grid(WorldGrid in_grid) { grid = in_grid; }

    /// <summary>
    /// Simple accesor for the tile grid.
    /// </summary>
    /// <returns>The tile grid.</returns>
    public WorldGrid Get_grid() { return grid; }
    public WorldNodeType Get_types() { return types; }
    //public WorldGeology Get_geology() { return geology; }
    public BiogeographicAmalgamator Get_biogeologicZones() { return biogeographicZones; }
    public WorldAnalyzer Get_analyzer() { return analyzer; }
    public WaterAmalgamateRevTwo Get_waterBodies() { return waterBodies; }
    public LandAmalgamate Get_landBodies() { return landBodies; }
    public RiverSystem Get_rivers() { return rivers; }

    /// <summary>
    /// Generates a world tile grid from parameters.
    /// </summary>
    /// <param name="in_type">Specifies world type.</param>
    /// <param name="in_rando">Inbound random number generator.</param>
    /// <param name="in_grid">Inbound, INITIALIZED grid.</param>
    /// <returns></returns>
    private WorldGrid Generate(int in_smoothing, System.Random in_rando, WorldGrid in_grid, GameData in_data)
    {
        WorldGrid toRet = grid;

        WorldDiamondSquare WDS = new WorldDiamondSquare(in_rando);
        toRet = WDS.GenerateNodes(toRet, in_data);

        int pos = 0;
        while (pos < in_smoothing)
        {
            WorldAreaSmoothing smoothing = new WorldAreaSmoothing();
            toRet = smoothing.GenerateNodes(toRet);
            pos++;
        }

        WorldSubGrid WSG = new WorldSubGrid();
        toRet = WSG.LerpNodes(toRet, in_data);

        types = new WorldNodeType();
        toRet = types.GenerateNodes(toRet, in_data);
        //toRet.Set_types(nodeType);

        waterBodies = new WaterAmalgamateRevTwo();
        toRet = waterBodies.GenerateNodes(toRet);

        landBodies = new LandAmalgamate();
        toRet = landBodies.GenerateNodes(toRet);
        Debug.Log("Largest landmass is " + landBodies.LargestCellCount() + " of " + landBodies.Total() + " total land nodes.");

        rivers = new RiverSystem();
        toRet = rivers.Generate(this, in_rando);

        SimplePrecipitation WP = new SimplePrecipitation();
        toRet = WP.Generate(toRet);

        SimpleTemperature ST = new SimpleTemperature();
        toRet = ST.Generate(toRet, in_data);

        biogeographicZones = new BiogeographicAmalgamator(in_data);
        toRet = biogeographicZones.Generate(toRet);

        return toRet;

    }

    // step building

    public World()
    {
    }

    public void BuildStep_Initialize(System.Random in_rando, GameData in_data)
    {
        //settings = in_data;
        grid = new WorldGrid(in_data.settings.nSize, in_rando);
        //biomes = new BiomeTable();
    }

    public void BuildStep_DiamondSquare(System.Random in_rando, GameData in_data)
    {
        WorldDiamondSquare WDS = new WorldDiamondSquare(in_rando);
        grid = WDS.GenerateNodes(grid, in_data);
    }

    public void BuildStep_Smoothing(GameData in_data)
    {
        int pos = 0;
        while (pos < in_data.settings.smoothingPasses)
        {
            WorldAreaSmoothing smoothing = new WorldAreaSmoothing();
            grid = smoothing.GenerateNodes(grid);
            pos++;
        }
    }

    public void BuildStep_SubGrid(GameData in_data)
    {
        WorldSubGrid WSG = new WorldSubGrid();
        grid = WSG.LerpNodes(grid, in_data);
    }

    public void BuildStep_NodeType(GameData in_data)
    {
        types = new WorldNodeType();
        grid = types.GenerateNodes(grid, in_data);
    }

    public void BuildStep_WaterAmalgamation()
    {
        waterBodies = new WaterAmalgamateRevTwo();
        grid = waterBodies.GenerateNodes(grid);
    }

    public void BuildStep_LandAmalgamation()
    {
        landBodies = new LandAmalgamate();
        grid = landBodies.GenerateNodes(grid);
        Debug.Log("Largest landmass is " + landBodies.LargestCellCount() + " of " + landBodies.Total() + " total land nodes.");
    }

    public void BuildStep_RiverSystem(System.Random in_rando)
    {
        rivers = new RiverSystem();
        grid = rivers.Generate(this, in_rando);
    }

    public void BuildStep_Climate(GameData in_data)
    {
        SimplePrecipitation WP = new SimplePrecipitation();
        grid = WP.Generate(grid);

        SimpleTemperature ST = new SimpleTemperature();
        grid = ST.Generate(grid, in_data);
    }

    public void BuildStep_Biogeography(GameData in_data)
    {
        biogeographicZones = new BiogeographicAmalgamator(in_data);
        grid = biogeographicZones.Generate(grid);
    }
}
