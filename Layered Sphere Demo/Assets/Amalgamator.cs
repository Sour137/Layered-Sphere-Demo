using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public abstract class Amalgamator
{
    protected List<IterableBinaryBubbleCell> cells;
    //protected bool[,] already;
    public Amalgamator()
    {

    }

    protected abstract bool NodeValid(WorldNode[,] in_node, Point2D in_point);
    protected abstract bool NodeValid(WorldNode[,] in_node, Point2D in_point, IterableBinaryBubbleCell in_cell);

    protected virtual List<IterableBinaryBubbleCell> NodeAmalgamate(WorldGrid in_grid, bool in_autoCorners)
    {
        cells = new List<IterableBinaryBubbleCell>();
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
                if (!already[x, y])
                {
                    if (NodeValid(nodes, new Point2D(x, y)))
                    {
                        already[x, y] = true;
                        IterableBinaryBubbleCell cell = new IterableBinaryBubbleCell(new Point2D(x, y), nodes);
                        List<Point2D> peri = cell.Get_perimeter();
                        int pos = NodeSelect(peri, nodes);
                        while (pos != -1)
                        {
                            Point2D select = peri[pos];
                            cell.Bubble(select);
                            already[select.x, select.y] = true;
                            pos = NodeSelect(peri, nodes);
                        }
                        if (in_autoCorners)
                            cell.AutoCorners();
                        cells.Add(cell);
                    }
                }
                x++;
            }
            x = 0;
            y++;
        }

        cells = SortCells(cells);
        return cells;
    }

    protected virtual List<IterableBinaryBubbleCell> ParallelNodeAmalgamate(WorldNode[,] in_nodes, List<IterableBinaryBubbleCell> in_cells)
    {
        bool[,] already = InitAlready(in_nodes);
        List<IterableBinaryBubbleCell> unfinished = in_cells;
        cells = new List<IterableBinaryBubbleCell>();
        int loops = 0;

        while (unfinished.Count > 0)
        {
            loops++;
            //Console.Write("Loop #" + loops + " Cell Count = " + cells.Count + " Finished = " + finished.Count);
            int pos = 0;
            int lim = unfinished.Count;
            while (pos < lim)
            {
                //Console.Write(" Cell Size : " + cells[pos].Get_interior().Count + "; ");
                bool mergeFlag = false;
                int nextPos = NodeSelect(ref mergeFlag, unfinished[pos].Get_perimeter(), in_nodes, already);
                if (nextPos == -1)
                {
                    cells.Add(unfinished[pos]);
                    unfinished.RemoveAt(pos);
                    lim = unfinished.Count;
                    //Console.WriteLine(finished.Count + " finished, " + lim + " remain");
                }
                else
                {
                    Point2D temp = unfinished[pos].Get_perimeter()[nextPos];
                    if (mergeFlag)
                    {
                        int mergeCellPos = -1;
                        if (PointInCells(ref mergeCellPos, temp, unfinished))
                        {
                            if ((mergeCellPos == -1) || mergeCellPos == pos)
                            {
                                Console.WriteLine("Merge fail!!!");
                                Console.ReadLine();
                            }
                            else
                            {
                                IterableBinaryBubbleCell toMerge = unfinished[mergeCellPos];
                                unfinished[pos].MergeCell(toMerge);
                                unfinished.RemoveAt(mergeCellPos);
                                lim = unfinished.Count;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("Clearing invalid already value...");
                            already[temp.x, temp.y] = false;
                        }
                    }
                    else
                    {
                        unfinished[pos].Bubble(temp);
                        already[temp.x, temp.y] = true;
                    }
                }
                pos++;
            }
            //Console.WriteLine();
        }
        //Console.Write(" verifying...");
        //finished = Verify(finished, already, in_grid);
        cells = SortCells(cells);
        return cells;
    }

    protected List<IterableBinaryBubbleCell> SortCells(List<IterableBinaryBubbleCell> in_cells)
    {
        List<IterableBinaryBubbleCell> unsorted = in_cells;
        List<IterableBinaryBubbleCell> sorted = new List<IterableBinaryBubbleCell>();
        while (unsorted.Count > 0)
        {
            int pos = 0;
            int highPos = 0;
            float highArea = 0f;
            while (pos < unsorted.Count)
            {
                float area = unsorted[pos].CurrentArea();
                if (area > highArea)
                {
                    highPos = pos;
                    highArea = area;
                }
                pos++;
            }
            sorted.Add(unsorted[highPos]);
            unsorted.RemoveAt(highPos);
        }
        return sorted;
    }

    protected List<IterableBinaryBubbleCell> SeedsToCells(List<Point2D> in_points, WorldGrid in_grid)
    {
        int pos = 0;
        int lim = in_points.Count;
        List<IterableBinaryBubbleCell> toRet = new List<IterableBinaryBubbleCell>();
        while (pos < lim)
        {
            IterableBinaryBubbleCell cell = new IterableBinaryBubbleCell(in_points[pos], in_grid);
            toRet.Add(cell);
            pos++;
        }
        return toRet;
    }

    protected bool[,] InitAlready(WorldNode[,] in_nodes)
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
                toRet[x, y] = false;
                x++;
            }
            x = 0;
            y++;
        }
        return toRet;
    }

    protected bool[,] InitAlready(WorldNode[,] in_nodes, List<IterableBinaryBubbleCell> in_cells)
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
                toRet[x, y] = false;
                x++;
            }
            x = 0;
            y++;
        }

        int pos = 0;
        while (pos < cells.Count)
        {
            List<Point2D> inner = in_cells[pos].Get_interior();
            int subPos = 0;
            while (subPos < inner.Count)
            {
                Point2D loc = inner[subPos];
                toRet[loc.x, loc.y] = true;
                subPos++;
            }
            pos++;
        }

        return toRet;
    }

    public bool PointInCells(ref int in_foundPos, Point2D in_point, List<IterableBinaryBubbleCell> in_cells)
    {
        bool toRet = false;
        int pos = 0;
        int lim = in_cells.Count;
        while (pos < lim)
        {
            int foundPos = -1;
            if (in_cells[pos].BinarySearch(in_point, ref foundPos, in_cells[pos].Get_interior()))
            {
                toRet = true;
                in_foundPos = pos;
                break;
            }
            else
                pos++;
        }
        return toRet;
    }

    protected bool PointInCells(ref int in_foundPos, Point2D in_point, List<IterableBinaryBubbleCell> in_cells, int in_skip)
    {
        bool toRet = false;
        int pos = 0;
        int lim = in_cells.Count;
        while (pos < lim)
        {
            if (pos != in_skip)
            {
                int foundPos = -1;
                if (in_cells[pos].BinarySearch(in_point, ref foundPos, in_cells[pos].Get_interior()))
                {
                    toRet = true;
                    in_foundPos = pos;
                    break;
                }
                else
                    pos++;
            }
            else
                pos++;
        }
        return toRet;
    }

    protected int NodeSelect(List<Point2D> in_peri, WorldNode[,] in_nodes)
    {
        int toRet = -1;
        int pos = 0;
        int lim = in_peri.Count;
        while (pos < lim)
        {
            if (NodeValid(in_nodes, in_peri[pos]))
            {
                toRet = pos;
                break;
            }
            pos++;
        }
        return toRet;
    }

    protected int NodeSelect(ref bool in_merge, List<Point2D> in_peri, WorldNode[,] in_nodes, bool[,] in_already)
    {
        in_merge = false;
        int toRet = -1;
        int pos = 0;
        int lim = in_peri.Count;
        while (pos < lim)
        {
            Point2D loc = in_peri[pos];
            if (NodeValid(in_nodes, in_peri[pos]))
            {
                if (!in_already[loc.x, loc.y])
                {
                    toRet = pos;
                    break;
                }
                else
                {
                    toRet = pos;
                    in_merge = true;
                    break;
                }
            }
            pos++;
        }
        return toRet;
    }

    protected int NodeSelect(List<Point2D> in_peri, WorldNode[,] in_nodes, IterableBinaryBubbleCell in_cell)
    {
        int toRet = -1;
        int pos = 0;
        int lim = in_peri.Count;
        while (pos < lim)
        {
            if (NodeValid(in_nodes, in_peri[pos], in_cell))
            {
                toRet = pos;
                break;
            }
            pos++;
        }
        return toRet;
    }

    public List<IterableBinaryBubbleCell> Get_rawCells() { return cells; }

    public int LargestCellCount()
    {
        int pos = 0;
        int largest = 0;
        while (pos < cells.Count)
        {
            if (cells[pos].Get_interior().Count > largest)
                largest = cells[pos].Get_interior().Count;
            pos++;
        }
        return largest;
    }

    public int Total()
    {
        int pos = 0;
        int total = 0;
        while (pos < cells.Count)
        {
            total += cells[pos].Get_interior().Count;
            pos++;
        }
        return total;
    }
}

