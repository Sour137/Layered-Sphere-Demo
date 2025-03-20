using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RiverSegment
{
    int x;
    int y;
    float volume;
    int riverID;

    public RiverSegment()
    {
        Set_XY(0, 0);
        Set_volume(0.0f);
        Set_riverID(-1);
    }

    public RiverSegment(int in_xPos, int in_yPos)
    {
        Set_XY(in_xPos, in_yPos);
        Set_volume(0.0f);
        Set_riverID(-1);

    }

    public RiverSegment(Point2D in_point)
    {
        Set_XY(in_point.x, in_point.y);
        Set_volume(0.0f);
        Set_riverID(-1);
    }

    public int Get_x() { return x; }
    public int Get_y() { return y; }
    public Point2D Get_Point()
    {
        Point2D toRet = new Point2D(x, y);
        return toRet;
    }
    public float Get_volume() { return volume; }
    public int Get_riverID() { return riverID; }

    public void Set_x(int in_x) { x = in_x; }
    public void Set_y(int in_y) { y = in_y; }
    public void Set_XY(int in_x, int in_y)
    {
        Set_x(in_x);
        Set_y(in_y);
    }
    public void Set_volume(float in_v) { volume = in_v; }
    public void Set_riverID(int in_val) { riverID = in_val; }
}

