using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


[System.Serializable] // we shouldn't serialize this
public class WorldTileArea
{
    public float outerCoefficient;
    public float innerCoefficient;
    public static int EARTHSURFACEAREA = 510065623;


    public WorldTileArea()
    {
        outerCoefficient = 0;
        innerCoefficient = 0;
    }

    public WorldTileArea(int in_maxX, int in_maxY)
    {
        outerCoefficient = CalcOuter(in_maxX, in_maxY);
        innerCoefficient = CalcInner(in_maxY);
    }

    public WorldTileArea(WorldNode[,] in_nodes)
    {
        int maxX = in_nodes.GetLength(0);
        int maxY = in_nodes.GetLength(1);
        outerCoefficient = CalcOuter(maxX, maxY);
        innerCoefficient = CalcInner(maxY);
    }

    public float TestSurfaceArea(int in_maxX, int in_maxY)
    {
        float toRet = 0;
        int y = 0;
        float runArea = 0;
        while (y < in_maxY)
        {
            runArea += (CalcArea(y) * (float)in_maxX);
            y++;
        }
        toRet = runArea;
        return toRet;
    }

    public float TestAccuracy(int maxX, int maxY)
    {
        float sa = TestSurfaceArea(maxX, maxY);
        float percentError = ((sa - EARTHSURFACEAREA) / EARTHSURFACEAREA) * 100;
        return percentError;
    }

    public float CalcArea(int in_y)
    {
        float inner = innerCoefficient * (float)in_y;
        float triggy = (float)(Math.Sin(inner));
        if (triggy == 0)
            triggy = 1;
        float toRet = outerCoefficient * triggy;
        return toRet;
    }

    private float CalcOuter(int maxX, int maxY)
    {
        double numer = 2.0 * Math.Pow((Math.PI * 6371), 2);
        double denom = maxX * maxY;
        float toRet = (float)(numer / denom);
        return toRet;
    }

    private float CalcInner(int maxY)
    {
        float toRet = (float)(Math.PI / (double)maxY);
        return toRet;
    }

}


