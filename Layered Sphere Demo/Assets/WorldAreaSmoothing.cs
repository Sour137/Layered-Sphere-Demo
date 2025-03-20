using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class WorldAreaSmoothing
{
    public WorldAreaSmoothing()
    {

    }

    //public WorldGrid Generate(WorldGrid in_grid)
    //{
    //    WorldGrid toRet = in_grid;

    //    int x = 0;
    //    int maxX = toRet.Get_maxX();
    //    int y = 0;
    //    int maxY = toRet.Get_maxY();

    //    while(y < maxY)
    //    {
    //        while(x < maxX)
    //        {
    //            Point2D loc = new Point2D(x, y);
    //            Point2D[] perimeter = loc.GetPerimeter();
    //            float avgElev = CalculateAvg(perimeter, toRet);
    //            ushort castElev = (ushort)avgElev;
    //            toRet.Get_tile(loc).Set_elevation(castElev);
    //            x++;
    //        }
    //        x = 0;
    //        y++;

    //    }

    //    toRet = ReProcessElevationCounts(toRet);// This counts the number of tiles at a certain elevation.

    //    return toRet;
    //}

    public WorldGrid GenerateNodes(WorldGrid in_grid)
    {
        WorldGrid toRet = in_grid;
        WorldNode[,] nodes = in_grid.Get_Nodes();
        PointValidator localValidator = new PointValidator(nodes);
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 0;
        int maxY = nodes.GetLength(1);

        while (y < maxY)
        {
            while (x < maxX)
            {
                Point2D loc = new Point2D(x, y);
                Point2D[] perimeter = loc.GetPerimeter();
                float avgElev = CalculateAvg(perimeter, nodes, localValidator);
                nodes[loc.x, loc.y].Set_elev(avgElev);
                x++;
            }
            x = 0;
            y++;

        }
        toRet.Set_nodes(nodes);
        //toRet = ReProcessElevationCounts(toRet);// This counts the number of tiles at a certain elevation.

        return toRet;
    }

    //private WorldGrid ReProcessElevationCounts(WorldGrid in_grid)
    //{
    //    WorldGrid toRet = in_grid;
    //    int highest = 0;
    //    int[] newElev = new int[in_grid.Get_elevationCounts().Length];
    //    newElev.Initialize();
    //    int x = 0;
    //    int maxX = in_grid.Get_maxX();
    //    int y = 0;
    //    int maxY = in_grid.Get_maxY();
    //    while (x < maxX)
    //    {
    //        while (y < maxY)
    //        {
    //            int elev = in_grid.Get_tile(x, y).Get_elevation();
    //            if (elev > highest)
    //                highest = elev;
    //            //Console.WriteLine(elev + " : " + newElev[elev]);
    //            newElev[elev] += 1;
    //            y++;
    //        }
    //        y = 0;
    //        x++;
    //    }
    //    newElev = TrimArray(newElev, highest);
    //    toRet.Set_elevationCounts(newElev);
    //    toRet.Set_highest(highest);
    //    //DBGOutElev(newElev, highest);
    //    return toRet;
    //}

    private int[] TrimArray(int[] in_set, int in_highest)
    {
        int pos = 0;
        int lim = CountActiveElements(in_set, in_highest);
        int[] toRet = new int[lim];

        while (pos < lim)
        {
            toRet[pos] = in_set[pos];
            pos++;
        }
        return toRet;
    }

    private int CountActiveElements(int[] in_set, int in_highest)
    {
        int pos = 0;
        int lim = in_set.Length;
        int last = -1;
        while (pos < lim)
        {
            if ((in_set[pos] == 0) && (last == 0) && (pos >= in_highest))
                break;
            else
            {
                last = in_set[pos];
                pos++;
            }
        }

        return pos;
    }

    //private float CalculateAvg(Point2D[] in_perimeter, WorldGrid in_grid)
    //{
    //    int pos = 0;
    //    int lim = in_perimeter.Length;
    //    float runSum = 0;
    //    int divies = 0;
    //    while(pos < lim)
    //    {
    //        runSum += in_grid.Get_tile(in_perimeter[pos]).Get_elevation();
    //        divies++;
    //        pos++;
    //    }
    //    float floatDiv = (float)divies;
    //    float toRet = runSum / floatDiv;
    //    return toRet;
    //}

    private float CalculateAvg(Point2D[] in_perimeter, WorldNode[,] in_nodes, PointValidator in_validator)
    {
        int pos = 0;
        int lim = in_perimeter.Length;
        float runSum = 0;
        int divies = 0;
        while (pos < lim)
        {
            Point2D loc = in_validator.ValidatePoint(in_perimeter[pos]);
            runSum += in_nodes[loc.x, loc.y].Get_elev();
            divies++;
            pos++;
        }
        float floatDiv = (float)divies;
        float toRet = runSum / floatDiv;
        return toRet;
    }
}

