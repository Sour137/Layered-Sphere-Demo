using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiogeographicZone : Cell
{
    public BiogeographicZone() : base()
    {

    }

    public BiogeographicZone(IterableBinaryBubbleCell in_solidified, WorldGrid in_grid) : base(in_solidified, in_grid)
    {

    }

    public Point2D[] Get_interior() { return base.Get_cell(); }
    public Point2D[] Get_perimeter() { return base.Get_bubble(); }
    public new float Get_surfaceArea() { return base.Get_surfaceArea(); }

}


