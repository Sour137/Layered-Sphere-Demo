using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LandCell : Cell
{
    Coast coast;

    public LandCell() : base()
    {

    }

    //public LandCell(BubbleCell in_solidified) : base(in_solidified)
    //{

    //}

    public LandCell(IterableBinaryBubbleCell in_solidified, WorldGrid in_grid) : base(in_solidified, in_grid)
    {

    }

    public LandCell(Cell in_base)
    {

    }

    public LandCell(IterableBinaryBubbleCell in_solidified, WorldNode[,] in_nodes) : base(in_solidified, in_nodes)
    {

    }

    public Point2D[] Get_LandPoints() { return base.Get_cell(); }
    public Point2D[] Get_CoastPoints() { return base.Get_bubble(); }
    public float Get_SurfaceArea() { return base.Get_surfaceArea(); }

    public Coast Get_coast() { return coast; }

    public void Set_coast(Coast in_coast) { coast = in_coast; }
}

