using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class WaterCell : Cell
{
    public WaterCell() : base()
    {

    }

    //public WaterCell(BubbleCell in_solidified) : base(in_solidified)
    //{

    //}

    public WaterCell(IterableBinaryBubbleCell in_solidified, WorldGrid in_grid) : base(in_solidified, in_grid)
    {

    }

    public WaterCell(IterableBinaryBubbleCell in_solidified, WorldNode[,] in_nodes) : base(in_solidified, in_nodes)
    {

    }

    public Point2D[] Get_WaterPoints() { return base.Get_cell(); }
    public Point2D[] Get_Coasts() { return base.Get_bubble(); }
    public float Get_SurfaceArea() { return base.Get_surfaceArea(); }
}

