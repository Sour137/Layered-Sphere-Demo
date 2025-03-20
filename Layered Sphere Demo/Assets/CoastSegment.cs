using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class CoastSegment
{
    Point2D coord;
    LandCell lands;
    char type;

    public CoastSegment()
    {
        Set_coordinate(new Point2D(-1, -1));
        Set_lands(null);
    }

    public CoastSegment(Point2D in_coord, LandCell in_root, char in_type)
    {
        Set_coordinate(in_coord);
        Set_lands(in_root);
        Set_type(in_type);
    }

    public Point2D Get_coordinate() { return coord; }
    public LandCell Get_lands() { return lands; }
    public char Get_type() { return type; }

    public void Set_coordinate(Point2D in_coord) { coord = in_coord; }
    public void Set_lands(LandCell in_lands) { lands = in_lands; }
    public void Set_type(char in_type) { type = in_type; }
}

