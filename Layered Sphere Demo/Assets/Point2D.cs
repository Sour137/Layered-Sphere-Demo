using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class Point2D
{
    const int PERIMETERLIM = 8;
    const int AXISLIM = 4;
    public int x;
    public int y;

    public Point2D()
    {
        x = -1;
        y = -1;
    }

    public Point2D(int in_x, int in_y)
    {
        x = in_x;
        y = in_y;
    }

    public Point2D(Vector2 ins)
    {
        x = RoundedInt(ins.x);
        y = RoundedInt(ins.y);
    }

    private int RoundedInt(float ins)
    {
        float remainder = ins % 1;
        int toRet = (int)ins;
        if (remainder > 0.5f)
            toRet++;
        return toRet;
    }

    public Point2D[] GetEdges()
    {
        Point2D[] toRet = new Point2D[AXISLIM];
        Point2D n = new Point2D(x, y - 1);
        Point2D e = new Point2D(x - 1, y);
        Point2D s = new Point2D(x, y + 1);
        Point2D w = new Point2D(x + 1, y);
        toRet[0] = n;
        toRet[1] = e;
        toRet[2] = s;
        toRet[3] = w;
        return toRet;
    }

    public Point2D[] GetCorners()
    {
        Point2D[] toRet = new Point2D[AXISLIM];
        int xLeft = x - 1;
        int xRight = x + 1;
        int yUp = y + 1;
        int yDown = y - 1;
        Point2D nw = new Point2D(xLeft, yUp);
        Point2D sw = new Point2D(xLeft, yDown);
        Point2D ne = new Point2D(xRight, yUp);
        Point2D se = new Point2D(xRight, yDown);
        toRet[0] = ne;
        toRet[1] = se;
        toRet[2] = sw;
        toRet[3] = nw;
        return toRet;
    }

    public Point2D[] GetPerimeter()
    {
        Point2D[] toRet = new Point2D[PERIMETERLIM];
        int xLeft = x - 1;
        int xRight = x + 1;
        int yUp = y + 1;
        int yDown = y - 1;
        Point2D nw = new Point2D(xLeft, yUp);
        Point2D w = new Point2D(xLeft, y);
        Point2D sw = new Point2D(xLeft, yDown);
        Point2D n = new Point2D(x, yUp);
        Point2D s = new Point2D(x, yDown);
        Point2D ne = new Point2D(xRight, yUp);
        Point2D e = new Point2D(xRight, y);
        Point2D se = new Point2D(xRight, yDown);
        toRet[0] = n;
        toRet[1] = ne;
        toRet[2] = e;
        toRet[3] = se;
        toRet[4] = s;
        toRet[5] = sw;
        toRet[6] = w;
        toRet[7] = nw;

        return toRet;
    }

    public Point2D[] GetValidatedPerimeter(PointValidator in_validator)
    {
        Point2D[] toRet = new Point2D[PERIMETERLIM];
        int xLeft = x - 1;
        int xRight = x + 1;
        int yUp = y + 1;
        int yDown = y - 1;
        Point2D nw = in_validator.ValidatePoint(new Point2D(xLeft, yUp));
        Point2D w = in_validator.ValidatePoint(new Point2D(xLeft, y));
        Point2D sw = in_validator.ValidatePoint(new Point2D(xLeft, yDown));
        Point2D n = in_validator.ValidatePoint(new Point2D(x, yUp));
        Point2D s = in_validator.ValidatePoint(new Point2D(x, yDown));
        Point2D ne = in_validator.ValidatePoint(new Point2D(xRight, yUp));
        Point2D e = in_validator.ValidatePoint(new Point2D(xRight, y));
        Point2D se = in_validator.ValidatePoint(new Point2D(xRight, yDown));
        toRet[0] = n;
        toRet[1] = ne;
        toRet[2] = e;
        toRet[3] = se;
        toRet[4] = s;
        toRet[5] = sw;
        toRet[6] = w;
        toRet[7] = nw;

        return toRet;
    }

    public Point2D[] GetValidatedEdges(PointValidator in_validator)
    {
        Point2D[] toRet = new Point2D[AXISLIM];
        Point2D n = in_validator.ValidatePoint(new Point2D(x, y - 1));
        Point2D e = in_validator.ValidatePoint(new Point2D(x - 1, y));
        Point2D s = in_validator.ValidatePoint(new Point2D(x, y + 1));
        Point2D w = in_validator.ValidatePoint(new Point2D(x + 1, y));
        toRet[0] = n;
        toRet[1] = e;
        toRet[2] = s;
        toRet[3] = w;
        return toRet;
    }

    public String GetStringForm()
    {
        String toRet = "(";
        toRet += x;
        toRet += ", ";
        toRet += y;
        toRet += ")";
        return toRet;
    }

    public bool IsEqualTo(Point2D ins)
    {
        if ((ins.x == x) && (ins.y == y))
            return true;
        else
            return false;
    }

        
}

