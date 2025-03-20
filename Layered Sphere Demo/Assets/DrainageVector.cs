using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainageVector
{
    float volume;
    int direction;
    int basinIndex;
    Point2D destination;
    Point2D[] drainageFor;

    public DrainageVector()
    {
        Set_volume(0f);
        Set_direction(-1);
        Set_basinIndex(-1);
        Set_destination(null);
        Set_drainageFor(new Point2D[0]);
    }

    public DrainageVector(int in_direction, Point2D in_destination)
    {
        Set_volume(0f);
        Set_direction(in_direction);
        Set_basinIndex(-1);
        Set_destination(in_destination);
        Set_drainageFor(new Point2D[0]);
    }

    public DrainageVector(Point2D[] in_drainageFor) // for terminals uninitialized from drainage builder.
    {
        Set_volume(0f);
        Set_direction(-1);
        Set_basinIndex(-1);
        Set_destination(null);
        Set_drainageFor(in_drainageFor);
    }

    public float Get_volume() { return volume; }
    public int Get_direction() { return direction; }
    public float Get_Angle()
    {
        float toRet = 0f;
        switch (direction)
        {
            case 1:
                toRet = 90f;
                break;
            case 2:
                toRet = 180f;
                break;
            case 3:
                toRet = 270f;
                break;
            default:
                toRet = 0f;
                break;
        }
        return toRet;
    }
    public Point2D Get_destination() { return destination; }
    public int Get_basinIndex() { return basinIndex; }
    public Point2D[] Get_drainageFor() { return drainageFor; }

    public void Set_volume(float ins) { volume = ins; }
    public void Set_direction(int ins) { direction = ins; }
    public void Set_basinIndex(int ins) { basinIndex = ins; }
    public void Set_destination(Point2D ins) { destination = ins; }
    public void Set_drainageFor(Point2D[] in_set) { drainageFor = in_set; }
    public void Add_drainageFor(Point2D[] in_set)
    {
        List<Point2D> temp = new List<Point2D>(drainageFor);
        int pos = 0;
        while (pos < in_set.Length)
        {
            if (!PointInSet(in_set[pos], temp.ToArray()))
                temp.Add(in_set[pos]);
            pos++;
        }
        Set_drainageFor(temp.ToArray());
    }

    public void CleanDrainage()
    {
        drainageFor = new Point2D[0];
    }

    private bool PointInSet(Point2D in_point, Point2D[] in_set)
    {
        bool toRet = false;
        int pos = 0;
        while (pos < in_set.Length)
        {
            Point2D loc = in_set[pos];
            if ((in_point.x == loc.x) && (in_point.y == loc.y))
            {
                toRet = true;
                break;
            }
            pos++;
        }
        return toRet;
    }

}


