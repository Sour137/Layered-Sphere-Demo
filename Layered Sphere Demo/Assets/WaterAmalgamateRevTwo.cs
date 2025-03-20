using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class WaterAmalgamateRevTwo : Amalgamator
{
    public WaterAmalgamateRevTwo()
    {

    }

    private WaterCell[] SolidifyAll(List<IterableBinaryBubbleCell> in_cells, WorldGrid in_grid)
    {
        int pos = 0;
        int lim = in_cells.Count;
        WaterCell[] toRet = new WaterCell[lim];
        while (pos < lim)
        {
            toRet[pos] = new WaterCell(in_cells[pos], in_grid);
            pos++;
        }
        return toRet;
    }

    private WaterCell[] SolidifyAll(List<IterableBinaryBubbleCell> in_cells, WorldNode[,] in_nodes)
    {
        int pos = 0;
        int lim = in_cells.Count;
        WaterCell[] toRet = new WaterCell[lim];
        while (pos < lim)
        {
            toRet[pos] = new WaterCell(in_cells[pos], in_nodes);
            pos++;
        }
        return toRet;
    }

    public WaterCell[] SolidifyAll(WorldNode[,] in_nodes)
    {
        int pos = 0;
        int lim = cells.Count;
        WaterCell[] toRet = new WaterCell[lim];
        while (pos < lim)
        {
            toRet[pos] = new WaterCell(cells[pos], in_nodes);
            pos++;
        }
        return toRet;
    }

    //public override WorldGrid Generate(WorldGrid in_grid, Random in_rando)
    //{
    //    WorldGrid toRet = in_grid;
    //    WaterCell[] solids = SolidifyAll(ParallelGenerate(toRet, in_rando), toRet);
    //    toRet.Set_largestWaterBody(GetLargest(solids));
    //    toRet.Set_waterBodies(solids);
    //    return toRet;
    //}

    public WorldGrid GenerateNodes(WorldGrid in_grid)
    {
        WorldGrid toRet = in_grid;
        NodeAmalgamate(in_grid, false);
        //toRet.Set_largestWaterBody(GetLargest(solids));
        //toRet.Set_waterBodies(solids);
        return toRet;
    }

    //protected override bool PointValid(WorldGrid in_grid, Point2D in_point)
    //{
    //    bool toRet = false;
    //    if (!in_grid.Get_tile(in_point).Get_isLand())
    //        toRet = true;
    //    return toRet;
    //}

    protected override bool NodeValid(WorldNode[,] in_node, Point2D in_point)
    {
        bool toRet = false;
        if (!in_node[in_point.x, in_point.y].IsLand())
            toRet = true;
        return toRet;
    }

    protected override bool NodeValid(WorldNode[,] in_node, Point2D in_point, IterableBinaryBubbleCell in_cell)
    {
        throw new NotImplementedException();
    }

    private float GetLargest(WaterCell[] in_solids)
    {
        float toRet = 0.0f;
        int pos = 0;
        int lim = in_solids.Length;
        while (pos < lim)
        {
            if (toRet < in_solids[pos].Get_SurfaceArea())
                toRet = in_solids[pos].Get_SurfaceArea();
            pos++;
        }

        return toRet;
    }


}

