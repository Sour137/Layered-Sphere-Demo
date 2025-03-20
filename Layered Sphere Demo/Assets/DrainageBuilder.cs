using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DrainageBuilder
{
    bool[,] already;
    PointValidator validator;
    public DrainageBuilder(World in_world)
    {
        WorldNode[,] param = in_world.Get_grid().Get_Nodes();
        already = InitAlready(param);
        validator = new PointValidator(param);
    }

    public WorldGrid GenerateNodes(WorldGrid in_grid, System.Random in_rando, WaterAmalgamateRevTwo in_waterBodies)
    {
        WorldGrid toRet = in_grid;
        List<IterableBinaryBubbleCell> cells = InitCells(toRet, in_waterBodies);
        bool expanded = true;
        int iterations = 0;
        while (expanded)
        {
            int pos = 0;
            expanded = false;
            while (pos < cells.Count)
            {
                IterableBinaryBubbleCell thisCell = cells[pos];
                if (ExpandCell(ref toRet, ref thisCell, in_rando, pos))
                {
                    cells[pos] = thisCell;
                    expanded = true;
                }
                pos++;
            }
            // This process is very expensive on n=6 worlds. not really actually
            iterations++;
        }
        //Debug.Log("Drainage Iterations required " + iterations + " with " + cells.Count + " cells");
        toRet = CheckTerminals(toRet);
        return toRet;
    }

    private WorldGrid CheckTerminals(WorldGrid in_grid)
    {
        WorldGrid toRet = in_grid;
        int x = 0;
        int maxX = already.GetLength(0);
        int y = 0;
        int maxY = already.GetLength(1);
        while (y < maxY)
        {
            while (x < maxX)
            {
                if (!already[x, y])
                    toRet.Get_node(x, y).Set_drainage(new DrainageVector());
                x++;
            }
            x = 0;
            y++;
        }
        return toRet;
    }


    private bool ExpandCell(ref WorldGrid theGrid, ref IterableBinaryBubbleCell theCell, System.Random in_rando, int in_pos /*dbg purposes only*/)
    {
        bool toRet = false;
        List<Point2D> perimeter = new List<Point2D>(theCell.Get_perimeter());
        List<Point2D> expanded = new List<Point2D>();
        List<Point2D> rejected = new List<Point2D>();
        while (perimeter.Count > 0)
        {
            int pos = in_rando.Next(0, perimeter.Count);
            Point2D loc = perimeter[pos];
            if (theGrid.Get_node(loc).IsLand())
            {
                Point2D[] locEdge = loc.GetEdges(); // If I do diagonal rivers I would change this
                int select = VectorSelect(theGrid, loc, in_rando, locEdge);

                already[loc.x, loc.y] = true;
                DrainageVector temp;
                if (select != -1)
                    temp = new DrainageVector(select, validator.ValidatePoint(locEdge[select]));
                else
                    temp = new DrainageVector(select, null);
                theGrid.Get_node(loc).Set_drainage(temp);
                expanded.Add(perimeter[pos]);
                perimeter.RemoveAt(pos);
                toRet = true;
            }
            else
            {
                already[loc.x, loc.y] = true;
                rejected.Add(perimeter[pos]);
                perimeter.RemoveAt(pos);
            }

        }
        int expand = 0;
        while (expand < expanded.Count)
        {
            theCell.Bubble(expanded[expand]);
            expand++;
        }
        return toRet;
    }

    private int VectorSelect(WorldGrid in_grid, Point2D in_point, System.Random in_rando, Point2D[] in_set)
    {
        Point2D[] edges = in_set;
        List<int> goodPoints = new List<int>();
        int pos = 0;
        float thisElev = in_grid.Get_node(in_point).Get_layerElev();
        bool lowerOnly = true;
        bool notAlreadyDone = true;
        while (pos < edges.Length)
        {
            Point2D validated = validator.ValidatePoint(edges[pos]);
            float edgeElev = in_grid.Get_node(validated).Get_layerElev();

            if (notAlreadyDone)
            {
                if (lowerOnly)
                {
                    if (edgeElev < thisElev)
                        goodPoints.Add(pos);
                }
                else
                {
                    if ((edgeElev < thisElev) || ((edgeElev == thisElev) && (already[validated.x, validated.y])))
                        goodPoints.Add(pos);
                }
            }
            else
            {
                if ((edgeElev < thisElev) || (edgeElev == thisElev))
                    goodPoints.Add(pos);
            }


            pos++;
            if ((lowerOnly) && (pos == edges.Length) && (goodPoints.Count == 0))
            {
                lowerOnly = false;
                pos = 0;
            }
            if ((!lowerOnly) && (pos == edges.Length) && (goodPoints.Count == 0) && (notAlreadyDone))
            {
                // if nothing else choose to make a level terminal
                notAlreadyDone = false;
                pos = 0;
            }
        }
        int toRet = -1;
        if (goodPoints.Count > 0)
            toRet = goodPoints[in_rando.Next(0, goodPoints.Count)];
        if (toRet == -1)
            Debug.Log("Obvious endoherric basin at " + in_point.GetStringForm());
        return toRet;
    }

    private List<IterableBinaryBubbleCell> InitCells(WorldGrid in_grid, WaterAmalgamateRevTwo in_waterBodies)
    {
        List<IterableBinaryBubbleCell> toRet = new List<IterableBinaryBubbleCell>();
        List<IterableBinaryBubbleCell> bodies = in_waterBodies.Get_rawCells();
        int pos = 0;
        while (pos < bodies.Count)
        {
            // hopefully this works
            toRet.Add(new IterableBinaryBubbleCell(bodies[pos].Get_interior(), bodies[pos].Get_perimeter(), in_grid));
            pos++;
        }
        return toRet;
    }

    private bool[,] InitAlready(WorldNode[,] in_nodes)
    {
        int x = 0;
        int y = 0;
        int maxX = in_nodes.GetLength(0);
        int maxY = in_nodes.GetLength(1);
        bool[,] toRet = new bool[maxX, maxY];
        while (y < maxY)
        {
            while (x < maxX)
            {
                if (in_nodes[x, y].IsLand())
                    toRet[x, y] = false;
                else
                    toRet[x, y] = true;
                x++;
            }
            x = 0;
            y++;
        }
        return toRet;
    }
}

