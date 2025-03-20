using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainageBasinAmalgamator : Amalgamator
{
    List<Point2D> loopbackPoints;
    public DrainageBasinAmalgamator()
    {
        loopbackPoints = new List<Point2D>();
    }

    protected override bool NodeValid(WorldNode[,] in_node, Point2D in_point)
    {
        throw new System.NotImplementedException();
    }

    protected override bool NodeValid(WorldNode[,] in_node, Point2D in_point, IterableBinaryBubbleCell in_cell)
    {
        throw new System.NotImplementedException();
    }

    private DrainageBasin[] SolidifyAll(List<IterableBinaryBubbleCell> in_cells, WorldGrid in_grid)
    {
        int pos = 0;
        int lim = in_cells.Count;
        DrainageBasin[] toRet = new DrainageBasin[lim];
        while (pos < lim)
        {
            toRet[pos] = new DrainageBasin(in_cells[pos], in_grid);
            pos++;
        }
        return toRet;
    }

    private DrainageBasin[] SolidifyAll(List<IterableBinaryBubbleCell> in_cells, WorldNode[,] in_nodes)
    {
        int pos = 0;
        int lim = in_cells.Count;
        DrainageBasin[] toRet = new DrainageBasin[lim];
        while (pos < lim)
        {
            toRet[pos] = new DrainageBasin(in_cells[pos], in_nodes);
            pos++;
        }
        return toRet;
    }

    public DrainageBasin[] SolidifyAll(WorldNode[,] in_nodes)
    {
        int pos = 0;
        int lim = cells.Count;
        DrainageBasin[] toRet = new DrainageBasin[lim];
        while (pos < lim)
        {
            toRet[pos] = new DrainageBasin(cells[pos], in_nodes);
            pos++;
        }
        return toRet;
    }

    private int GetLargestIndex()
    {
        int toRet = 0;
        int pos = 0;
        int largest = 0;
        while (pos < cells.Count)
        {
            if (cells[pos].Get_interior().Count > largest)
            {
                largest = cells[pos].Get_interior().Count;
                toRet = pos;
            }
            pos++;
        }

        return toRet;
    }

    // Business section

    public WorldGrid GenerateNodes(WorldGrid in_grid, System.Random in_rando)
    {
        // tested with 715937206 1865648423(endoherric)
        WorldGrid toRet = in_grid;
        // Amalgamation Section
        List<IterableBinaryBubbleCell> rawBasins = NodeAmalgamate(toRet, false);
        // New loopback section 7/23
        // loopback correction would work by repeatedly setting drainages checking the known loopback cells
        // then identifying the loopback points by drainage counts, adding those points to a list
        // checking their edges and adding valid edges to a list that will be randomly selected from
        // cells are merged, and drainages are recalculated for that cell then repeat for the next cell. 
        bool changed = true;
        toRet = SetDrainages(rawBasins, in_grid); // early assignment needed for loopbacks
        int flipsPerformed = 0;
        while (changed)
        {
            changed = false;
            int pos = 0;
            while (pos < rawBasins.Count)
            {
                if (!DrainsToOcean(toRet, rawBasins[pos]))
                {
                    List<Point2D> loopbacks = CollectLoopbacks(toRet, rawBasins[pos]);
                    List<Point2D> flipableLoopbacks = FlipableLoopbacks(toRet, loopbacks, pos);
                    if (flipableLoopbacks.Count > 0)
                    {
                        // at this point we have detected that the cell has at least one terminal with an edge
                        // that does not belong to this cell. Random select -> merge cells -> SetDrainage
                        int flipPoint = in_rando.Next(flipableLoopbacks.Count);
                        Point2D[] edges = flipableLoopbacks[flipPoint].GetValidatedEdges(new PointValidator(toRet.Get_Nodes()));
                        int vectorSelect = VectorSelect(edges, pos, in_grid, in_rando);
                        int mergeIndex = PointInCells(edges[vectorSelect], rawBasins);
                        if (mergeIndex != pos)
                        {
                            if (mergeIndex > -1)
                            {
                                flipsPerformed++;
                                //Debug.Log("Flip performed at " + flipableLoopbacks[flipPoint].GetStringForm());
                                // flip it
                                DrainageVector temp = new DrainageVector(vectorSelect, edges[vectorSelect]);
                                toRet.Get_node(flipableLoopbacks[flipPoint]).Set_drainage(temp);
                                // merge cells and set drainages again
                                rawBasins[pos].MergeCell(rawBasins[mergeIndex]);
                                rawBasins.RemoveAt(mergeIndex);
                                if (mergeIndex < pos)
                                    pos--;
                                changed = true;
                            }
                            toRet = SetDrainage(pos, rawBasins[pos], in_grid);
                        }
                    }
                }
                pos++;
            }
        }
        //Debug.Log(flipsPerformed + " flips performed");
        //// Loopback Section
        //toRet = ProcessLoopbacks(toRet, rawBasins, in_rando);
        //List<IterableBinaryBubbleCell> finalBasins = AmalgamateLoopbackCells(rawBasins, toRet);
        // Assignment Section
        cells = ProcessCorners(rawBasins);
        toRet = SetDrainages(cells, toRet);
        int largest = GetLargestIndex();
        float area = cells[largest].CurrentArea();
        Debug.Log("The largest drainage is #" + largest + " with " + cells[largest].Get_interior().Count + " nodes at a drainage area of " + area + " km2");
        return toRet;
    }

    private int VectorSelect(Point2D[] in_edges, int in_basinIndex, WorldGrid in_grid, System.Random in_rando)
    {
        int pos = 0;
        List<int> validChoices = new List<int>();
        while (pos < in_edges.Length)
        {
            if (!in_grid.Get_node(in_edges[pos]).IsLand())
                validChoices.Add(pos);
            else if (in_grid.Get_node(in_edges[pos]).Get_drainage().Get_basinIndex() != in_basinIndex)
                validChoices.Add(pos);
            pos++;
        }

        int toRet = -1;
        if (validChoices.Count > 0)
        {
            int select = in_rando.Next(0, validChoices.Count);
            toRet = validChoices[select];
        }
        return toRet;
    }

    private List<Point2D> FlipableLoopbacks(WorldGrid in_grid, List<Point2D> in_loopbacks, int in_index)
    {
        List<Point2D> toRet = new List<Point2D>();

        int pos = 0;
        while (pos < in_loopbacks.Count)
        {
            float locElev = in_grid.Get_node(in_loopbacks[pos]).Get_layerElev();
            Point2D[] locEdges = in_loopbacks[pos].GetValidatedEdges(new PointValidator(in_grid.Get_Nodes()));
            int subPos = 0;

            while (subPos < locEdges.Length)
            {
                Point2D loc = in_loopbacks[pos];
                float edgeElev = in_grid.Get_node(locEdges[subPos]).Get_layerElev();

                if (edgeElev <= locElev)
                {
                    int locId = in_grid.Get_node(loc).Get_drainage().Get_basinIndex();
                    int edgeID = in_grid.Get_node(locEdges[subPos]).Get_drainage().Get_basinIndex();
                    if (edgeID != locId)
                    {
                        if (!PointInSet(loc, toRet.ToArray()))
                            toRet.Add(in_loopbacks[pos]);
                        break;
                    }
                }
                subPos++;
            }
            pos++;
        }

        return toRet;
    }

    private List<Point2D> CollectLoopbacks(WorldGrid in_grid, IterableBinaryBubbleCell in_cell)
    {
        List<Point2D> toRet = new List<Point2D>();
        List<Point2D> inner = in_cell.Get_interior();
        int highCount = GetHighest(in_grid, in_cell);
        int pos = 0;
        while (pos < inner.Count)
        {
            int locCount = in_grid.Get_node(inner[pos]).Get_drainage().Get_drainageFor().Length;
            if (locCount == highCount)
                toRet.Add(inner[pos]);
            pos++;
        }
        return toRet;
    }

    private int GetHighest(WorldGrid in_grid, IterableBinaryBubbleCell in_cell)
    {
        List<Point2D> inner = in_cell.Get_interior();
        int highCount = 0;
        int pos = 0;
        while (pos < inner.Count)
        {
            int locCount = in_grid.Get_node(inner[pos]).Get_drainage().Get_drainageFor().Length;
            if (locCount > highCount)
                highCount = locCount;
            pos++;
        }
        return highCount;
    }

    private bool DrainsToOcean(WorldGrid in_grid, IterableBinaryBubbleCell in_cell)
    {
        bool toRet = false;

        Point2D loc = in_cell.Get_interior()[0];
        Point2D[] downstream = GetDownstreamPoints(in_grid.Get_Nodes(), loc);
        Point2D terminal;
        if (downstream.Length == 0)
            terminal = loc;
        else
            terminal = downstream[downstream.Length - 1];
        WorldNode terminalNode = in_grid.Get_node(terminal);
        DrainageVector locVector = terminalNode.Get_drainage();
        if (locVector != null)
            if (locVector.Get_destination() != null)
            {
                WorldNode terminalDestination = in_grid.Get_node(locVector.Get_destination());
                if (!terminalDestination.IsLand())
                    toRet = true;
            }


        return toRet;
    }

    private WorldGrid SetDrainage(int in_basinIndex, IterableBinaryBubbleCell in_cell, WorldGrid in_grid)
    {
        WorldGrid toRet = CleanDrainages(in_grid, in_cell);
        List<Point2D> points = in_cell.Get_interior();
        int pos = 0;
        while (pos < points.Count)
        {
            Point2D loc = points[pos];
            toRet.Get_node(loc).Get_drainage().Set_basinIndex(in_basinIndex);
            Point2D[] downstream = GetDownstreamPoints(in_grid.Get_Nodes(), loc);
            List<Point2D> current = new List<Point2D>();
            current.Add(loc);
            int subPos = 0;
            while (subPos < downstream.Length)
            {
                loc = downstream[subPos];
                toRet.Get_node(loc).Get_drainage().Add_drainageFor(current.ToArray());
                toRet.Get_node(loc).Get_drainage().Set_basinIndex(in_basinIndex);
                current.Add(loc);
                subPos++;
            }
            pos++;
        }
        return toRet;
    }

    private WorldGrid CleanDrainages(WorldGrid in_grid, IterableBinaryBubbleCell in_cell)
    {
        WorldGrid toRet = in_grid;
        int pos = 0;
        List<Point2D> inner = in_cell.Get_interior();
        while (pos < inner.Count)
        {
            Point2D loc = inner[pos];
            toRet.Get_node(loc).Get_drainage().CleanDrainage();
            pos++;
        }

        return toRet;
    }

    private List<IterableBinaryBubbleCell> ProcessCorners(List<IterableBinaryBubbleCell> in_cells)
    {
        List<IterableBinaryBubbleCell> toRet = new List<IterableBinaryBubbleCell>();
        int pos = 0;
        while (pos < in_cells.Count)
        {
            in_cells[pos].AutoCorners();
            toRet.Add(in_cells[pos]);
            pos++;
        }
        return toRet;
    }

    // Amalgamation Section

    protected override List<IterableBinaryBubbleCell> NodeAmalgamate(WorldGrid in_grid, bool in_autoCorners)
    {
        List<IterableBinaryBubbleCell> basins = new List<IterableBinaryBubbleCell>();
        WorldNode[,] nodes = in_grid.Get_Nodes();
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 0;
        int maxY = nodes.GetLength(1);
        bool[,] already = InitAlready(nodes);
        while (y < maxY)
        {
            while (x < maxX)
            {
                if ((!already[x, y]) && (nodes[x, y].IsLand()))
                {
                    // current pos has not been amalgamated
                    IterableBinaryBubbleCell thisDrainage = new IterableBinaryBubbleCell(new Point2D(x, y), in_grid);
                    already[x, y] = true;
                    // follow the drainage down towards the ocean or another already amalgamated drainage
                    Point2D[] downstream = GetDownstreamPoints(nodes, new Point2D(x, y));
                    int pos = 0;
                    bool mergeFlag = false;
                    while (pos < downstream.Length)
                    {
                        Point2D loc = downstream[pos];
                        //if ((loc.x == 120) && (loc.y == 27))
                        //{
                        //    Debug.Log("Test point detected in downstream");
                        //}
                        if (!already[loc.x, loc.y])
                        {
                            // adding this location to this drainage
                            thisDrainage.Bubble(loc);
                            already[loc.x, loc.y] = true;
                        }
                        else
                        {
                            // this location has already been amalgamated and we must find that in cells
                            int subPos = 0;
                            while (subPos < basins.Count)
                            {
                                int foundPos = 0;
                                if (basins[subPos].BinarySearch(loc, ref foundPos, basins[subPos].Get_interior()))
                                    break;
                                else
                                    subPos++;
                            }
                            if (subPos < basins.Count)
                            {
                                // merge this drainage with the found drainage
                                basins[subPos].MergeCell(thisDrainage);
                                //Debug.Log("Merging basin with #" + subPos + " of " + cells.Count);
                                mergeFlag = true;
                                break;
                            }
                            else
                            {
                                int foundPos = 0;
                                if (thisDrainage.BinarySearch(loc, ref foundPos, thisDrainage.Get_interior()))
                                {
                                    //Debug.Log("Loopback from " + loc.GetStringForm());
                                    loopbackPoints.Add(loc);
                                    // this identifies at least some of the loopbacks, maybe not all of them tho.
                                    break;
                                }
                                else
                                {
                                    // unreachable? already has been set but the point does not exist in any cell
                                    Debug.Log("Unreachable Error from " + loc.GetStringForm());
                                    break;
                                }

                            }
                        }
                        pos++;
                    }
                    // then add it to the list
                    if (!mergeFlag)
                        basins.Add(thisDrainage);
                }
                x++;
            }
            x = 0;
            y++;
        }

        //cells = SortCells(cells);
        return basins;
    }

    private Point2D[] GetDownstreamPoints(WorldNode[,] in_node, Point2D in_point)
    {
        List<Point2D> downstream = new List<Point2D>();
        if (in_node[in_point.x, in_point.y].Get_drainage().Get_destination() != null)
        {
            Point2D next = in_node[in_point.x, in_point.y].Get_drainage().Get_destination();
            while ((in_node[next.x, next.y].IsLand()) && (!PointInSet(next, downstream.ToArray())))
            {
                downstream.Add(next);
                if (in_node[next.x, next.y].Get_drainage() != null)
                    if (in_node[next.x, next.y].Get_drainage().Get_destination() != null)
                        next = in_node[next.x, next.y].Get_drainage().Get_destination();
                    else
                    {
                        break;
                    }
                else break;
            }
        }
        return downstream.ToArray();

    }

    // Terminal Flipping Section
    private WorldGrid ProcessLoopbacks(WorldGrid in_grid, List<IterableBinaryBubbleCell> in_basins, System.Random in_rando)
    {
        WorldGrid toRet = in_grid;
        PointValidator validator = new PointValidator(toRet.Get_Nodes());
        List<int> loopbackLocs = FindLoopbackPoints(in_basins);
        int pos = 0;

        while (pos < loopbackPoints.Count)
        {
            // this loop passes through all the loopback points previously identified and will try to drain them into a basin that does not contain a loopback point
            Point2D loc = loopbackPoints[pos];
            Point2D[] edges = loc.GetEdges();
            List<Point2D> mergeableEdges = new List<Point2D>();
            int subPos = 0;
            while (subPos < edges.Length)
            {
                // check to see if the 4 edge points do not contain a loopback point in their cell and add to list
                Point2D valid = validator.ValidatePoint(edges[subPos]);
                if (!SkipPoint(valid, loopbackLocs, in_basins))
                    mergeableEdges.Add(valid);
                subPos++;
            }
            int edgeSelection = -1;
            if (mergeableEdges.Count > 0)
            {
                // randomly select between valid edges
                edgeSelection = in_rando.Next(0, mergeableEdges.Count - 1);
                // and set the destination in the node's vector
                toRet.Get_node(loc).Get_drainage().Set_destination(mergeableEdges[edgeSelection]);
                // and change the direction
                toRet.Get_node(loc).Get_drainage().Set_direction(CalculateNewDirection(mergeableEdges[edgeSelection], loc));
                //Debug.Log("Loopback at " + loopbackPoints[pos].GetStringForm() + " flipped to new direction");
            }
            //else
            //    Debug.Log("Loopback at " + loopbackPoints[pos].GetStringForm() + " was unable to be flipped...");

            pos++;
        }


        return toRet;
    }

    private List<int> FindLoopbackPoints(List<IterableBinaryBubbleCell> in_cells)
    {
        List<int> toRet = new List<int>();
        int pos = 0;
        while (pos < loopbackPoints.Count)
        {
            toRet.Add(PointInCells(loopbackPoints[pos], in_cells));
            pos++;
        }
        return toRet;
    }

    private int PointInCells(Point2D in_point, List<IterableBinaryBubbleCell> in_cells)
    {
        int pos = 0;
        while (pos < in_cells.Count)
        {
            int foundPos = 0;
            if (in_cells[pos].BinarySearch(in_point, ref foundPos, in_cells[pos].Get_interior()))
                break;
            else
                pos++;
        }
        if (pos == in_cells.Count)
            pos = -1;
        return pos;
    }

    private bool SkipPoint(Point2D in_toTest, List<int> in_loopbackLocs, List<IterableBinaryBubbleCell> in_cells)
    {
        bool toRet = false;
        // this is true if the point toTest is in the skip basins
        int foundPos = PointInCells(in_toTest, in_cells);
        int pos = 0;
        while (pos < in_loopbackLocs.Count)
        {
            if (foundPos == in_loopbackLocs[pos])
            {
                toRet = true;
                break;
            }
            pos++;
        }
        return toRet;
    }

    private int CalculateNewDirection(Point2D in_destination, Point2D in_loc)
    {
        Point2D delta = new Point2D(in_destination.x - in_loc.x, in_destination.y - in_loc.y);
        int toRet = -1;
        if (delta.y == 0)
        {
            if (delta.x == 1)
                toRet = 3;
            else
                toRet = 1;
        }
        else if (delta.y == 1)
            toRet = 2;
        else if (delta.y == -1)
            toRet = 0;

        return toRet;

    }

    private List<IterableBinaryBubbleCell> AmalgamateLoopbackCells(List<IterableBinaryBubbleCell> in_rawBasins, WorldGrid in_grid)
    {
        List<IterableBinaryBubbleCell> toRet = new List<IterableBinaryBubbleCell>(in_rawBasins);
        int pos = 0;
        while (pos < loopbackPoints.Count)
        {
            Point2D loc = loopbackPoints[pos];
            int locPos = PointInCells(loc, toRet);
            Point2D destination = in_grid.Get_node(loc).Get_drainage().Get_destination();
            int destinationPos = PointInCells(destination, toRet);
            if (locPos != destinationPos)
            {
                toRet[destinationPos].MergeCell(toRet[locPos]);
                toRet.RemoveAt(locPos);
                //Debug.Log("Loopback at " + locPos + ":" + loc.GetStringForm() + " merged with " + destinationPos + ":" + destination.GetStringForm() + " with a total basin count of " + toRet.Count + " from a raw count of " + in_rawBasins.Count);
            }
            pos++;
        }


        return toRet;
    }

    // Drainage Section

    private WorldGrid SetDrainages(List<IterableBinaryBubbleCell> in_cells, WorldGrid in_grid)
    {
        // game plan now is to do a scanline particle drop like we did last for the amalgamation section
        // and have duplicate prevention integrated into the vector through something like an add funk
        WorldGrid toRet = in_grid;
        WorldNode[,] nodes = toRet.Get_Nodes();
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 0;
        int maxY = nodes.GetLength(1);
        while (y < maxY)
        {
            while (x < maxX)
            {
                if (nodes[x, y].IsLand())
                {
                    Point2D loc = new Point2D(x, y);
                    int basinIndex = PointInCells(loc, in_cells);
                    toRet.Get_node(loc).Get_drainage().Set_basinIndex(basinIndex);
                    //if (basinIndex == -1)
                    //    Debug.Log(loc.GetStringForm() + " was not found in cells");
                    Point2D[] downstream = GetDownstreamPoints(nodes, loc);
                    List<Point2D> current = new List<Point2D>();
                    current.Add(loc);
                    int pos = 0;
                    while (pos < downstream.Length)
                    {
                        loc = downstream[pos];
                        toRet.Get_node(loc).Get_drainage().Add_drainageFor(current.ToArray());
                        toRet.Get_node(loc).Get_drainage().Set_basinIndex(basinIndex);
                        current.Add(loc);
                        pos++;
                    }
                }
                x++;
            }
            x = 0;
            y++;
        }

        return toRet;
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


