using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrainageBasin : Cell
{
    public DrainageBasin() : base()
    {

    }

    public DrainageBasin(IterableBinaryBubbleCell in_solidified, WorldGrid in_grid) : base(in_solidified, in_grid)
    {

    }

    public DrainageBasin(IterableBinaryBubbleCell in_solidified, WorldNode[,] in_nodes) : base(in_solidified, in_nodes)
    {

    }

    public Point2D[] Get_BasinPoints() { return base.Get_cell(); }
    public Point2D[] Get_EdgePoints() { return base.Get_bubble(); }
}


