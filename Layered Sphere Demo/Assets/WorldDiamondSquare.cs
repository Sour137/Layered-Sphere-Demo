using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class WorldDiamondSquare
{
    Random rando;
    const int LIM_CORNER = 5;
    int highest;
    //int maxElev;

    public WorldDiamondSquare()
    {
        rando = new Random();
        highest = 0;
    }

    public WorldDiamondSquare(Random in_rando)
    {
        highest = 0;
        rando = in_rando;
    }

    //public WorldGrid Generate(WorldGrid in_grid)
    //{
    //    WorldGrid toRet = in_grid;
    //    int maxElev = toRet.Get_MaxElevation();
    //    //Province[,] loc = in_grid.Get_tiles();
    //    int lim = in_grid.Get_depth();
    //    int iteration = 0;
    //    while (iteration <= lim)
    //    {
    //        int distance = Get_Distance(iteration, toRet);
    //        if(distance > 0)
    //        {
    //            toRet = SquareStep(distance, toRet, maxElev);
    //            toRet = DiamondStep(distance, toRet, maxElev);
    //        }
    //        iteration++;
    //        //Console.WriteLine("iterate...");
    //        //if (Program.DEBUG)
    //        //    Console.Write(iteration + ":" + distance + " ");
    //    }
    //    //toRet.Set_tiles(loc);
    //    toRet.Set_highest(highest);
    //    return toRet;
    //}

    public WorldGrid GenerateNodes(WorldGrid in_grid, GameData in_data)
    {
        WorldGrid toRet = in_grid;
        WorldNode[,] nodes = toRet.Get_Nodes();
        PointValidator localValidator = new PointValidator(nodes);
        int limElev = in_grid.Get_MaxElevation();
        int lim = in_data.settings.nSize;
        int iteration = 0;
        while (iteration <= lim)
        {
            int distance = Get_Distance(iteration, toRet);
            if (distance > 0)
            {
                nodes = NodeSquareStep(distance, nodes, limElev, localValidator);
                nodes = NodeDiamondStep(distance, nodes, limElev, localValidator);
            }
            iteration++;
        }
        toRet.Set_nodes(nodes);
        toRet.Set_highest(highest);
        return toRet;
    }

    //private WorldGrid SquareStep(int in_distance, WorldGrid in_grid, int in_randoLimit)
    //{
    //    WorldGrid toRet = in_grid;
    //    Point2D[] corners = Get_SquareCorners(in_distance);
    //    while(GoodEdge(corners, in_grid, 'y'))
    //    {
    //        Point2D[] goBackCorners = corners;
    //        while(GoodEdge(corners, in_grid, 'x'))
    //        {
    //            //Console.Write("Inside...");
    //            toRet = ProcessThem(toRet, corners, in_randoLimit);
    //            corners = Move_SquareCornersRight(in_distance, corners);
    //        }
    //        //Console.Write(" Outside ");
    //        corners = Move_SquareCornersDown(in_distance, goBackCorners);
    //    }
    //    //Console.WriteLine("Leaving Square...");
    //    return toRet;
    //}

    private WorldNode[,] NodeSquareStep(int in_distance, WorldNode[,] in_nodes, int in_randoLimit, PointValidator in_validator)
    {
        WorldNode[,] toRet = in_nodes;
        Point2D[] corners = Get_SquareCorners(in_distance);
        int maxX = toRet.GetLength(0);
        int maxY = toRet.GetLength(1);
        while (corners[2].y <= maxY)
        {
            Point2D[] goBackCorners = corners;
            while (corners[2].x <= maxX)
            {
                toRet = ProcessNodes(toRet, corners, in_randoLimit, in_validator);
                corners = Move_SquareCornersRight(in_distance, corners);
            }
            corners = Move_SquareCornersDown(in_distance, goBackCorners);
        }
        return toRet;
    }

    //private WorldGrid DiamondStep(int in_distance, WorldGrid in_grid, int in_randoLimit)
    //{
    //    WorldGrid toRet = in_grid;
    //    Point2D[] corners = Get_DiamondCorners(in_distance);
    //    while(GoodEdge(corners[2], in_grid, 'y'))
    //    {
    //        Point2D[] goBackCorners = corners;
    //        while(GoodEdge(corners[4], in_grid, 'x'))
    //        {
    //            toRet = ProcessThem(toRet, corners, in_randoLimit);
    //            corners = Move_SquareCornersRight(in_distance, corners);
    //        }
    //        corners = Move_SquareCornersDown(in_distance, goBackCorners);
    //    }

    //    return toRet;
    //}

    private WorldNode[,] NodeDiamondStep(int in_distance, WorldNode[,] in_nodes, int in_randoLimit, PointValidator in_validator)
    {
        WorldNode[,] toRet = in_nodes;
        Point2D[] corners = Get_DiamondCorners(in_distance);
        int maxX = toRet.GetLength(0);
        int maxY = toRet.GetLength(1);
        while (corners[2].y <= maxY)
        {
            Point2D[] goBackCorners = corners;
            while (corners[4].x <= maxX)
            {
                toRet = ProcessNodes(toRet, corners, in_randoLimit, in_validator);
                corners = Move_SquareCornersRight(in_distance, corners);
            }
            corners = Move_SquareCornersDown(in_distance, goBackCorners);
        }
        return toRet;
    }

    //private WorldGrid ProcessThem(WorldGrid in_grid, Point2D[] in_point, int in_randoLimit)
    //{
    //    WorldGrid toRet = in_grid;
    //    int pos = 0;
    //    int terms = 0;
    //    int sum = 0;
    //    while(pos < LIM_CORNER)
    //    {
    //        Point2D loc = in_point[pos];
    //        //Console.WriteLine("(" + loc.x + ", " + loc.y + ")... ");
    //        char tileType = toRet.Get_tile(loc).Get_elevationType();
    //        if(pos < (LIM_CORNER - 1))
    //        {

    //            if(tileType == '0')
    //            {
    //                toRet.Get_tile(loc).Set_elevationType('E');
    //                toRet.Get_tile(loc).Set_elevation(Get_Elevation(rando, in_randoLimit));
    //                toRet.Add_elevationCounts(toRet.Get_tile(loc).Get_elevation());
    //                sum += toRet.Get_tile(loc).Get_elevation();
    //                terms++;
    //            }
    //            else if (tileType == 'E')
    //            {
    //                sum += toRet.Get_tile(loc).Get_elevation();
    //                terms++;
    //            }
    //            else
    //            {
    //                Console.WriteLine("Early Nope!");
    //                Console.ReadLine();
    //            }
    //        }
    //        else
    //        {
    //            if(tileType == '0')
    //            {

    //                toRet.Get_tile(loc).Set_elevationType('E');
    //                toRet.Get_tile(loc).Set_elevation(Get_Elevation(rando, sum, terms));
    //                toRet.Add_elevationCounts(toRet.Get_tile(loc).Get_elevation());
    //                sum = 0;
    //                terms = 0;
    //                pos = LIM_CORNER;
    //            }
    //            else if(tileType == 'E')
    //            {
    //                toRet.Get_tile(loc).Set_elevation(Get_Elevation(rando, sum, terms));
    //                sum = 0;
    //                terms = 0;
    //                pos = LIM_CORNER;
    //            }
    //            else
    //            {
    //                Console.WriteLine("The Worrisome Nope!");
    //                Console.WriteLine(pos + " : (" + loc.x + ", " + loc.y + ") is a " + toRet.Get_tile(loc).Get_elevationType() + " with and elevation of " + toRet.Get_tile(loc).Get_elevation());
    //                Console.ReadLine();
    //            }
    //        }
    //        pos++;
    //    }
    //    return toRet;
    //}

    private WorldNode[,] ProcessNodes(WorldNode[,] in_nodes, Point2D[] in_corners, int in_randoLimit, PointValidator in_validator)
    {
        WorldNode[,] toRet = in_nodes;
        int pos = 0;
        int terms = 0;
        float sum = 0;
        while (pos < LIM_CORNER)
        {
            Point2D loc = in_validator.ValidatePoint(in_corners[pos]);
            //Console.WriteLine("(" + loc.x + ", " + loc.y + ")... ");
            char tileType = toRet[loc.x, loc.y].Get_elevType();
            if (pos < (LIM_CORNER - 1))
            {

                if (tileType == '0')
                {
                    toRet[loc.x, loc.y].Set_elevType('E');
                    toRet[loc.x, loc.y].Set_elev(Get_Elevation(rando, in_randoLimit));
                    //toRet.Add_elevationCounts(toRet.Get_tile(loc).Get_elevation());
                    sum += toRet[loc.x, loc.y].Get_elev();
                    terms++;
                }
                else if (tileType == 'E')
                {
                    sum += toRet[loc.x, loc.y].Get_elev();
                    terms++;
                }
            }
            else
            {
                if (tileType == '0')
                {

                    toRet[loc.x, loc.y].Set_elevType('E');
                    toRet[loc.x, loc.y].Set_elev(Get_Elevation(rando, sum, terms));
                    //toRet.Add_elevationCounts(toRet.Get_tile(loc).Get_elevation());
                    sum = 0;
                    terms = 0;
                    pos = LIM_CORNER;
                }
                else if (tileType == 'E')
                {
                    toRet[loc.x, loc.y].Set_elev(Get_Elevation(rando, sum, terms));
                    sum = 0;
                    terms = 0;
                    pos = LIM_CORNER;
                }
            }
            pos++;
        }
        return toRet;
    }

    private bool GoodEdge(Point2D[] in_points, WorldGrid in_grid, char choice)
    {
        bool toRet = false;
        Point2D lowerRight = in_points[2];
        switch (choice)
        {
            case 'x':
            case 'X':
                if (lowerRight.x <= in_grid.Get_Nodes().GetLength(0))
                    toRet = true;
                break;
            case 'y':
            case 'Y':
                if (lowerRight.y <= in_grid.Get_Nodes().GetLength(1))
                    toRet = true;
                break;
            default:
                break;
        }
        return toRet;
    }

    private bool GoodEdge(Point2D in_point, WorldGrid in_grid, char choice)
    {
        bool toRet = false;
        switch (choice)
        {
            case 'x':
            case 'X':
                if (in_point.x <= in_grid.Get_Nodes().GetLength(0))
                    toRet = true;
                break;
            case 'y':
            case 'Y':
                if (in_point.y <= in_grid.Get_Nodes().GetLength(1))
                    toRet = true;
                break;
            default:
                break;
        }
        return toRet;
    }

    private int Get_Distance(int in_iteration, WorldGrid in_grid)
    {
        double modifier = 1 / Math.Pow(2, (in_iteration));
        int distance = (int)(modifier * (in_grid.Get_Nodes().GetLength(1) - 1));
        return distance;
    }

    private Point2D[] Get_SquareCorners(int in_distance)
    {
        Point2D[] toRet = new Point2D[LIM_CORNER];
        Point2D upLeft = new Point2D(0, 0); // Upper left is origin. Indexed at origin.
        Point2D upRight = new Point2D(in_distance, 0);
        Point2D lowRight = new Point2D(in_distance, in_distance);
        Point2D lowLeft = new Point2D(0, in_distance);
        Point2D center = new Point2D((in_distance / 2), (in_distance / 2));
        toRet[0] = upLeft;
        toRet[1] = upRight;
        toRet[2] = lowRight;
        toRet[3] = lowLeft;
        toRet[4] = center;
        return toRet;
    }

    private Point2D[] Get_DiamondCorners(int in_distance)
    {
        int halfDist = in_distance / 2;
        Point2D[] toRet = new Point2D[LIM_CORNER];
        Point2D up = new Point2D(0, 0); //Upper corner of diamond is indexed to origin.
        Point2D right = new Point2D(halfDist, halfDist);
        Point2D down = new Point2D(0, in_distance);
        Point2D left = new Point2D((-1 * halfDist), halfDist);
        Point2D center = new Point2D(0, halfDist);
        toRet[0] = up;
        toRet[1] = right;
        toRet[2] = down;
        toRet[3] = left;
        toRet[4] = center;

        return toRet;
    }

    private ushort Get_Elevation(Random in_rando, int in_limit)
    {
        ushort toRet = (ushort)in_rando.Next(0, in_limit);

        CheckHigh(toRet);
        //Console.WriteLine("Elevation = " + toRet);
        return toRet;
    }

    private ushort Get_Elevation(Random in_rando, float in_sum, float in_terms)
    {
        int avg = (int)(in_sum / in_terms);
        int displacement = in_rando.Next(0, 2);
        ushort toRet = (ushort)(displacement + avg);
        CheckHigh(toRet);
        //Console.WriteLine("Elevation = " + toRet + " = " + in_sum + " / " + in_terms + " + " + displacement);
        return toRet;
    }

    private void CheckHigh(int in_val)
    {
        if (in_val > highest)
            highest = in_val;
    }

    private Point2D[] Move_SquareCornersRight(int in_distance, Point2D[] in_corners)
    {
        Point2D[] toRet = new Point2D[LIM_CORNER];
        int pos = 0;
        while (pos < LIM_CORNER)
        {
            int x = in_corners[pos].x + in_distance;
            int y = in_corners[pos].y;
            Point2D temp = new Point2D(x, y);
            toRet[pos] = temp;
            pos++;
        }
        return toRet;
    }

    private Point2D[] Move_SquareCornersDown(int in_distance, Point2D[] in_points)
    {
        Point2D[] toRet = new Point2D[LIM_CORNER];
        int pos = 0;
        while (pos < LIM_CORNER)
        {
            int x = in_points[pos].x;
            int y = in_points[pos].y + in_distance;
            Point2D temp = new Point2D(x, y);
            toRet[pos] = temp;
            pos++;
        }
        return toRet;
    }
}

