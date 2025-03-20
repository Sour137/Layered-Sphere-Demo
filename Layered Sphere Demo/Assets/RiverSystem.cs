using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverSystem
{
    DrainageBasinAmalgamator drainages;
    public RiverSystem()
    {

    }

    public WorldGrid Generate(World in_world, System.Random in_rando)
    {
        WorldGrid toRet = GenerateDrainages(in_world, in_rando);

        return toRet;
    }

    private WorldGrid GenerateDrainages(World in_world, System.Random in_rando)
    {
        WorldGrid toRet = in_world.Get_grid();
        WaterAmalgamateRevTwo bodies = in_world.Get_waterBodies();
        DrainageBuilder builder = new DrainageBuilder(in_world);
        toRet = builder.GenerateNodes(toRet, in_rando, bodies);
        // time to amalgamate (<< 7/22)
        drainages = new DrainageBasinAmalgamator();
        toRet = drainages.GenerateNodes(toRet, in_rando);
        return toRet;
    }

    public DrainageBasinAmalgamator Get_drainages() { return drainages; }

}
