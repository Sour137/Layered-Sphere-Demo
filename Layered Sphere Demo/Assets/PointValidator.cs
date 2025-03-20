using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


[System.Serializable]// we shouldn't serialize these
public class PointValidator
{
    public int maxX;
    public int maxY;

    public PointValidator()
    {
        maxX = 0;
        maxY = 0;
    }

    //public PointValidator(WorldGrid in_grid)
    //{
    //    maxX = in_grid.Get_maxX();
    //    maxY = in_grid.Get_maxY();
    //}

    public PointValidator(WorldNode[,] in_nodes)
    {
        maxX = in_nodes.GetLength(0);
        maxY = in_nodes.GetLength(1);
    }

    public PointValidator(int[,] in_map)
    {
        maxX = in_map.GetLength(0);
        maxY = in_map.GetLength(1);
    }

    public PointValidator(bool[,] in_map)
    {
        maxX = in_map.GetLength(0);
        maxY = in_map.GetLength(1);
    }

    public int Get_maxX() { return maxX; }
    public int Get_maxY() { return maxY; }

    public Point2D ValidatePoint(int in_a, int in_b, int in_maxA, int in_maxB)
    {
        int a = in_a;
        int b = in_b;
        Point2D toRet = new Point2D(a, b);
        if (BoundsFail(a, in_maxA, b, in_maxB))
        {
            if (BFail(a, in_maxA))
            {
                if (a == in_maxA)
                    a = 0;
                else
                    a = ValidatePos(a, in_maxA);
            }
            if (BFail(b, in_maxB))
            {
                if (b == in_maxB)
                    b = 0;
                else
                    b = ValidatePos(b, in_maxB);
            }
            toRet = new Point2D(a, b);
        }
        return toRet;
    }

    public Point2D ValidatePoint(int in_x, int in_y)
    {
        int x = in_x;
        int y = in_y;
        Point2D toRet = new Point2D(x, y);
        if (BoundsFail(in_x, in_y))
        {
            if (YBoundFail(y))
            {
                //toRet = SphericalYShift(toRet); Not going to use y shifting.
                x = toRet.x;
                //y = toRet.y;
                //y = ValidatePos(toRet.y, maxY);
                if (y >= maxY)
                    y = maxY - 1;
                else
                    y = 0;
            }
            if (XBoundFail(x))
            {
                if (x == maxX)
                    x = 0;
                else
                    x = ValidatePos(x, maxX);
            }
            toRet = new Point2D(x, y);
        }
        return toRet;
    }

    public Point2D ValidatePoint(Point2D in_point)
    {
        return ValidatePoint(in_point.x, in_point.y);
    }

    public int ValidatePos(int in_pos, int in_max)
    {
        int toRet = in_pos;
        if (toRet >= in_max)
            while ((toRet >= in_max) && (toRet > 0))
            {
                toRet -= in_max;
            }
        if (toRet < 0)
            while ((toRet < 0) && (toRet < in_max))
            {
                toRet += in_max;
            }
        return toRet;
    }

    public bool BoundsFail(int in_a, int in_maxA, int in_b, int in_maxB)
    {
        bool toRet = false;
        if ((BFail(in_a, in_maxA)) || (BFail(in_b, in_maxB)))
            toRet = true;
        return toRet;
    }

    public bool BoundsFail(int in_x, int in_y)
    {
        bool toRet = false;
        if ((XBoundFail(in_x)) || (YBoundFail(in_y)))
            toRet = true;
        else
            toRet = false;
        return toRet;
    }

    public bool BFail(int in_a, int in_maxA)
    {
        bool toRet = false;
        if ((in_a < 0) || (in_a > in_maxA))
            toRet = true;
        return toRet;
    }


    public bool XBoundFail(int in_x)
    {
        bool toRet = false;
        if ((in_x < 0) || (in_x >= maxX))
            toRet = true;
        return toRet;
    }

    public bool YBoundFail(int in_y)
    {
        bool toRet = false;
        if ((in_y < 0) || (in_y >= maxY))
            toRet = true;
        return toRet;
    }

    private Point2D SphericalYShift(Point2D in_point)
    {
        int y = in_point.y;
        if (y < 0)
            y = Math.Abs(y + 1);
        else if (y >= maxY)
            y = 2 * maxY - y - 1;
        int x = in_point.x + (int)((float)maxX / 2.0f);
        //if (BoundsFail(in_x, in_maxX))
        //    in_x = ValidatePos(in_x, in_maxX);
        if (YBoundFail(y))
        {
            Console.WriteLine("Something went wrong..." + y + '/' + maxY + " from " + in_point.y);
            Console.ReadLine();
        }

        return new Point2D(x, y);
    }
}

