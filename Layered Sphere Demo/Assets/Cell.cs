using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class Cell
{
    protected Point2D[] cell;
    protected Point2D[] bubble;
    float surfaceArea;
    //County[] counties;

    public Cell()
    {
        cell = new Point2D[0];
        bubble = new Point2D[0];
        surfaceArea = 0.0f;
    }

    //public Cell(BubbleCell in_solidified)
    //{
    //    cell = in_solidified.Get_interior().ToArray();
    //    bubble = in_solidified.Get_perimeter().ToArray();
    //    surfaceArea = in_solidified.Get_squareKm();
    //}

    public Cell(IterableBinaryBubbleCell in_solidified, WorldGrid in_grid)
    {
        cell = in_solidified.Get_interior().ToArray();
        bubble = Combine(in_solidified.Get_perimeter(), in_solidified.Get_rejects()).ToArray();
        surfaceArea = CalculateSA(in_grid.Get_Nodes());
    }

    public Cell(IterableBinaryBubbleCell in_solidified, WorldNode[,] in_nodes)
    {
        cell = in_solidified.Get_interior().ToArray();
        bubble = Combine(in_solidified.Get_perimeter(), in_solidified.Get_rejects()).ToArray();
        surfaceArea = CalculateSA(in_nodes);
    }

    public Cell(IterableBinaryBubbleCell in_solidified, WorldTileArea in_area)
    {
        cell = in_solidified.Get_interior().ToArray();
        bubble = Combine(in_solidified.Get_perimeter(), in_solidified.Get_rejects()).ToArray();
        surfaceArea = CalculateSA(in_area);
    }

    protected Point2D[] Get_cell() { return cell; }
    protected Point2D[] Get_bubble() { return bubble; }
    protected float Get_surfaceArea() { return surfaceArea; }
    //public County[] Get_counties() { return counties; }

    //public void Set_counties(County[] in_counties) { counties = in_counties; }


    private List<Point2D> Combine(List<Point2D> in_peri, List<Point2D> in_reject)
    {
        List<Point2D> toRet = in_peri;
        int pos = 0;
        int lim = in_reject.Count;
        while (pos < lim)
        {
            toRet.Add(in_reject[pos]);
            pos++;
        }
        return toRet;
    }

    private float CalculateSA(WorldNode[,] in_nodes)
    {
        int pos = 0;
        int lim = cell.Length;
        float toRet = 0.0f;
        WorldTileArea locAreaFunk = new WorldTileArea(in_nodes);
        while (pos < lim)
        {
            toRet += locAreaFunk.CalcArea(cell[pos].y);
            pos++;
        }

        return toRet;
    }

    private float CalculateSA(WorldTileArea in_area)
    {
        int pos = 0;
        int lim = cell.Length;
        float toRet = 0.0f;
        while (pos < lim)
        {
            toRet += in_area.CalcArea(cell[pos].y);
            pos++;
        }

        return toRet;
    }
}

