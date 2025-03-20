using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class IterableBinaryBubbleCell
{
    public List<Point2D> interior;
    public List<Point2D> perimeter;
    public List<Point2D> validPerimeter;
    public List<Point2D> reject;
    public List<Point2D> corners;
    public List<Corner> cornerStates;
    public PointValidator validator;
    public WorldTileArea areaFunk;

    public List<Point2D> Get_interior() { return interior; }
    public List<Point2D> Get_perimeter() { return perimeter; }
    public List<Point2D> Get_validPerimeter() { return validPerimeter; }
    public List<Point2D> Get_rejects() { return reject; }
    public List<Point2D> Get_corners() { return corners; }
    public List<Corner> Get_cornerStates() { return cornerStates; }

    public IterableBinaryBubbleCell(Point2D in_seed, WorldGrid in_grid)
    {
        reject = new List<Point2D>();
        interior = new List<Point2D>();
        interior.Add(in_seed);
        //validator = in_grid.Get_validator();
        validator = new PointValidator(in_grid.Get_Nodes());
        perimeter = SortedList(in_seed);
        validPerimeter = new List<Point2D>();
        corners = new List<Point2D>();
        cornerStates = new List<Corner>();
        areaFunk = new WorldTileArea(in_grid.Get_Nodes());
    }

    public IterableBinaryBubbleCell(List<Point2D> in_interior, List<Point2D> in_perimeter, WorldGrid in_grid)
    {
        reject = new List<Point2D>();
        interior = in_interior;
        perimeter = in_perimeter;
        validPerimeter = new List<Point2D>();
        cornerStates = new List<Corner>();
        // may need to add bool for corner interpret at this point
        //validator = in_grid.Get_validator();
        validator = new PointValidator(in_grid.Get_Nodes());
        areaFunk = new WorldTileArea(in_grid.Get_Nodes());

    }

    public IterableBinaryBubbleCell(Point2D in_seed, WorldNode[,] in_nodes)
    {
        reject = new List<Point2D>();
        interior = new List<Point2D>();
        interior.Add(in_seed);
        validator = new PointValidator(in_nodes);
        perimeter = SortedList(in_seed);
        validPerimeter = new List<Point2D>();
        corners = new List<Point2D>();
        cornerStates = new List<Corner>();
        areaFunk = new WorldTileArea(in_nodes);

    }

    /// <summary>
    /// Takes a point from within the perimeter and inserts it into the interior, removing it from the perimeter.
    /// </summary>
    /// <param name="in_fromPeri">The point that IS IN perimeter.</param>
    public void Bubble(Point2D in_fromPeri)
    {
        int foundPos = -1;
        if (BinarySearch(in_fromPeri, ref foundPos, perimeter))
        {
            //if (in_fromPeri.IsEqualTo(new Point2D(1, 51)))
            //    Debug.Log("Test Point detected at Bubble()");
            perimeter.RemoveAt(foundPos);
            SortedInsertInterior(in_fromPeri);
            ExpandPerimeter(in_fromPeri);
        }
        if (BinarySearch(in_fromPeri, ref foundPos, validPerimeter))
            validPerimeter.RemoveAt(foundPos);
    }

    public void Burst(Point2D in_fromInterior)
    {
        // this removes an interior point and rebuilds the perimeter list
        // seems expensive, try not to call frequently.
        int foundPos = -1;
        if (BinarySearch(in_fromInterior, ref foundPos, interior))
        {
            interior.RemoveAt(foundPos);
            RebuildPerimeter();
        }
    }

    private void RebuildPerimeter()
    {
        List<Point2D> toProcess = new List<Point2D>(interior);
        perimeter = new List<Point2D>();
        if (toProcess.Count > 0)
        {
            ExpandPerimeter(toProcess[0]);
            toProcess.RemoveAt(0);
        }
        else
            Debug.Log("Can't rebuild perimeter from a 0 sized interior cell");
        while (toProcess.Count > 0)
        {
            int pos = 0;
            while (pos < interior.Count)
            {
                int foundPos = -1;
                if (BinarySearch(toProcess[pos], ref foundPos, perimeter))
                {
                    perimeter.RemoveAt(foundPos);
                    ExpandPerimeter(toProcess[pos]);
                    toProcess.RemoveAt(pos);
                }
                pos++;
            }
        }

    }

    public bool BubbleBool(Point2D in_fromPeri)
    {
        bool toRet = false;
        int foundPos = -1;
        if (BinarySearch(in_fromPeri, ref foundPos, perimeter))
        {
            toRet = true;
            perimeter.RemoveAt(foundPos);
            SortedInsertInterior(in_fromPeri);
            ExpandPerimeter(in_fromPeri);
        }
        return toRet;
    }

    public void ForceBubble(Point2D in_point)
    {
        SortedInsertInterior(in_point);
        ExpandPerimeter(in_point);
    }

    public void MergeCell(IterableBinaryBubbleCell toMerge)
    {
        List<Point2D> otherInner = toMerge.Get_interior();
        int pos = 0;
        int lim = otherInner.Count;
        while (pos < lim)
        {
            int periPos = -1;
            if (BinarySearch(otherInner[pos], ref periPos, perimeter))
                perimeter.RemoveAt(periPos);
            SortedInsertInterior(otherInner[pos]);
            ExpandPerimeter(otherInner[pos]);
            pos++;
        }
    }

    protected List<Point2D> SortedList(Point2D in_seed)
    {
        List<Point2D> toSort = in_seed.GetPerimeter().ToList<Point2D>();
        List<Point2D> toRet = new List<Point2D>();
        int pos = 0;
        int count = 0;
        int lim = toSort.Count;
        int hardLim = lim;
        while (count < hardLim)
        {
            Point2D lowest = new Point2D(int.MaxValue, int.MaxValue);
            int lowPos = int.MaxValue;
            pos = 0;
            while (pos < lim)
            {
                Point2D toCompare = validator.ValidatePoint(toSort[pos]);
                if ((toCompare.x < lowest.x) || ((toCompare.x == lowest.x) && (toCompare.y < lowest.y)))
                {
                    //Program.OutCoord(toCompare);
                    //Console.Write("<");
                    //Program.OutCoord(lowest);
                    //Console.Write(", ");
                    lowest = toCompare;
                    lowPos = pos;
                }
                else
                {
                    //Program.OutCoord(toCompare);
                    //Console.Write('>');
                    //Program.OutCoord(lowest);
                    //Console.Write(", ");
                }
                pos++;
            }
            //Console.WriteLine();
            toRet.Add(lowest);
            toSort.RemoveAt(lowPos);
            lim = toSort.Count;
            count++;
        }

        return toRet;
    }

    public void SortedInsertInterior(Point2D in_point)
    {
        interior = SortedInsert(in_point, interior);
    }

    public void ExpandPerimeter(Point2D in_seed)
    {
        List<Point2D> toAdd = in_seed.GetPerimeter().ToList<Point2D>();
        List<Point2D> prePeri = perimeter;
        int pos = 0;
        int lim = toAdd.Count;
        int foundPos = 0;
        while (pos < lim)
        {
            Point2D thePoint = validator.ValidatePoint(toAdd[pos]);
            //Console.WriteLine("Expanding Perimeter...");
            if (!BinarySearch(thePoint, ref foundPos, interior))
            {
                if (!BinarySearch(thePoint, ref foundPos, prePeri))
                {
                    if (!BinarySearch(thePoint, ref foundPos, reject))
                        prePeri = SortedInsert(thePoint, prePeri);
                    //Console.WriteLine("Inserting into perimeter!");
                }
                //else
                //    Console.WriteLine("Point is already in perimeter...");
            }
            //else
            //    Console.WriteLine("Point is already in interior...");
            pos++;
        }
        perimeter = prePeri;
    }

    public void ExpandValidPerimeter(List<Point2D> in_toAdd)
    {
        // points received will need to be pre-validated based on the object using the cell.
        // valid here meaning the points are good to be bubbled when the iteration comes up and not that they are validated using the point validator
        // in order for them to be pre-validated they will still need to use the pointvalidator on the cell holder side
        int pos = 0;
        int foundPos = 0;
        List<Point2D> preList = validPerimeter;
        while (pos < in_toAdd.Count)
        {
            Point2D thePoint = in_toAdd[pos];
            if (!BinarySearch(thePoint, ref foundPos, interior))
            {
                if (!BinarySearch(thePoint, ref foundPos, preList))
                {
                    if (!BinarySearch(thePoint, ref foundPos, reject))
                        preList = SortedInsert(thePoint, preList);
                }
            }
            pos++;
        }
        validPerimeter = preList;
    }

    public void RejectPeriPos(int pos)
    {
        reject.Add(perimeter[pos]);
        perimeter.RemoveAt(pos);
    }

    public List<Point2D> SortedInsert(Point2D in_point, List<Point2D> in_list)
    {
        List<Point2D> toRet = in_list;
        int pos = 0;
        int lim = toRet.Count;
        while (pos < lim)
        {
            if (PointHigher(in_point, toRet[pos]))
                pos++;
            else
                break;
        }
        toRet.Insert(pos, in_point);
        return toRet;
    }

    public List<Point2D> SortedInsert(Point2D in_point, List<Point2D> in_list, ref int insertPos)
    {
        List<Point2D> toRet = in_list;
        int pos = 0;
        int lim = toRet.Count;
        while (pos < lim)
        {
            if (PointHigher(in_point, toRet[pos]))
                pos++;
            else
                break;
        }
        insertPos = pos;
        toRet.Insert(insertPos, in_point);
        return toRet;
    }

    public bool FindCorner(Point2D in_loc, int in_corner)
    {
        bool toRet = false;
        int foundPos = -1;
        if (BinarySearch(in_loc, ref foundPos, corners))
        {
            switch (in_corner)
            {
                case 0:
                    toRet = cornerStates[foundPos].nw;
                    break;
                case 1:
                    toRet = cornerStates[foundPos].ne;
                    break;
                case 2:
                    toRet = cornerStates[foundPos].se;
                    break;
                case 3:
                    toRet = cornerStates[foundPos].sw;
                    break;
                default:
                    break;
            }
        }
        return toRet;
    }

    public void AutoCorners()
    {
        // presumes the interior points are finished
        // will likely be used on everything except for things that need to see the city layer
        // adds corners for a node based off of interior data.
        corners = new List<Point2D>();
        cornerStates = new List<Corner>();
        bool[,] active = InitActiveMap();
        int x = 0;
        int maxX = validator.Get_maxX();
        int y = 1;
        int maxY = validator.Get_maxY();
        while (y < maxY - 1)
        {
            while (x < maxX)
            {
                Point2D loc = new Point2D(x, y);
                bool[] locActive = GetLocActive(loc, active);
                int[] locConfigs = GetConfigs(loc, locActive, maxY);
                bool nw = false;
                bool ne = false;
                bool se = false;
                bool sw = false;
                if ((NWCornerActive(locConfigs[0])))
                    nw = true;
                if ((NECornerActive(locConfigs[1])))
                    ne = true;
                if ((SECornerActive(locConfigs[2])))
                    se = true;
                if ((SWCornerActive(locConfigs[3])))
                    sw = true;
                if ((nw) || (ne) || (se) || (sw))
                {
                    int insertPos = -1;
                    corners = SortedInsert(loc, corners, ref insertPos);
                    cornerStates.Insert(insertPos, new Corner(nw, ne, se, sw));
                }

                x++;
            }
            x = 0;
            y++;
        }

    }

    private bool NWCornerActive(int in_config)
    {
        bool toRet = false;

        if ((in_config == 3) ||
            (in_config == 5) ||
            (in_config == 6) ||
            (in_config == 7) ||
            (in_config == 10) ||
            (in_config == 11) ||
            (in_config == 13) ||
            (in_config == 14) ||
            (in_config == 15))
            toRet = true;

        return toRet;
    }

    private bool NECornerActive(int in_config)
    {
        bool toRet = false;

        if ((in_config == 3) ||
            (in_config == 5) ||
            (in_config == 7) ||
            (in_config == 9) ||
            (in_config == 10) ||
            (in_config == 11) ||
            (in_config == 13) ||
            (in_config == 14) ||
            (in_config == 15))
            toRet = true;
        return toRet;
    }

    private bool SECornerActive(int in_config)
    {
        bool toRet = false;

        if ((in_config == 5) ||
            (in_config == 7) ||
            (in_config == 9) ||
            (in_config == 10) ||
            (in_config == 11) ||
            (in_config == 12) ||
            (in_config == 13) ||
            (in_config == 14) ||
            (in_config == 15))
            toRet = true;
        return toRet;
    }

    private bool SWCornerActive(int in_config)
    {
        bool toRet = false;

        if ((in_config == 5) ||
            (in_config == 6) ||
            (in_config == 7) ||
            (in_config == 10) ||
            (in_config == 11) ||
            (in_config == 12) ||
            (in_config == 13) ||
            (in_config == 14) ||
            (in_config == 15))
            toRet = true;
        return toRet;
    }

    private bool[] GetLocActive(Point2D in_center, bool[,] in_active)
    {
        bool[] toRet = new bool[9];
        Point2D[] peri = in_center.GetValidatedPerimeter(new PointValidator(in_active));
        int pos = 0;
        while (pos < peri.Length)
        {
            Point2D loc = peri[pos];
            toRet[pos] = in_active[loc.x, loc.y];
            pos++;
        }
        toRet[8] = in_active[in_center.x, in_center.y];
        return toRet;
    }

    private int[] GetConfigs(Point2D in_center, bool[] in_active, int in_maxY)
    {
        // marker can delete
        int[] toRet = new int[4];
        int nw;
        int ne;
        int se;
        int sw;

        if (in_center.y == 0)
        {
            nw = ConfigFromNodes(in_active[8], in_active[8], in_active[8], in_active[6]); // c c C w
            ne = ConfigFromNodes(in_active[8], in_active[8], in_active[2], in_active[8]); // c c e C
            se = ConfigFromNodes(in_active[8], in_active[2], in_active[3], in_active[4]); // C e se s
            sw = ConfigFromNodes(in_active[6], in_active[8], in_active[4], in_active[5]); // w C s sw
        }
        else if ((in_center.y + 1) == in_maxY)
        {
            nw = ConfigFromNodes(in_active[7], in_active[0], in_active[8], in_active[6]); // nw n C w
            ne = ConfigFromNodes(in_active[0], in_active[1], in_active[2], in_active[8]); // n ne e C
            se = ConfigFromNodes(in_active[8], in_active[2], in_active[8], in_active[8]); // C e c c
            sw = ConfigFromNodes(in_active[6], in_active[8], in_active[8], in_active[8]); // w C c c
        }
        else
        {
            nw = ConfigFromNodes(in_active[7], in_active[0], in_active[8], in_active[6]); // nw n C w
            ne = ConfigFromNodes(in_active[0], in_active[1], in_active[2], in_active[8]); // n ne e C
            se = ConfigFromNodes(in_active[8], in_active[2], in_active[3], in_active[4]); // C e se s
            sw = ConfigFromNodes(in_active[6], in_active[8], in_active[4], in_active[5]); // w C s sw
        }

        toRet[0] = nw;
        toRet[1] = ne;
        toRet[2] = se;
        toRet[3] = sw;


        //toRet[0] = 3;
        //toRet[1] = 3;
        //toRet[2] = 3;
        //toRet[3] = 3;

        return toRet;
    }

    private int ConfigFromNodes(bool in_nw, bool in_ne, bool in_se, bool in_sw)
    {
        int configuration = 0;
        if (in_nw/* && (in_nw.Get_layerElev() == in_layerElev)*/)
            configuration += 8;
        if (in_ne/* && (in_ne.Get_layerElev() == in_layerElev)*/)
            configuration += 4;
        if (in_se/* && (in_se.Get_layerElev() == in_layerElev)*/)
            configuration += 2;
        if (in_sw/* && (in_sw.Get_layerElev() == in_layerElev)*/)
            configuration += 1;

        //if ((configuration == 15) && ((NodeOccluded(in_layerElev, in_nw.Get_layerElev())) && (NodeOccluded(in_layerElev, in_ne.Get_layerElev())) && (NodeOccluded(in_layerElev, in_se.Get_layerElev())) && (NodeOccluded(in_layerElev, in_sw.Get_layerElev()))))
        //    configuration = 0;

        return configuration;
    }

    //private bool NodeActive(float in_thisLayer, WorldNode in_node)
    //{
    //    bool toRet = false;
    //    if (in_node.Get_layerElev() >= in_thisLayer)
    //        toRet = true;
    //    return toRet;
    //}

    //private bool NodeOccluded(float in_elev, float in_locLayerElev)
    //{
    //    bool toRet = false;
    //    if (in_locLayerElev > in_elev)
    //        toRet = true;
    //    return toRet;
    //}

    private bool[,] InitActiveMap()
    {
        bool[,] toRet = InitAlready();
        int pos = 0;
        while (pos < interior.Count)
        {
            Point2D loc = interior[pos];
            toRet[loc.x, loc.y] = true;
            pos++;
        }
        return toRet;
    }

    private bool[,] InitAlready()
    {
        int x = 0;
        int y = 0;
        int maxX = validator.Get_maxX();
        int maxY = validator.Get_maxY();
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

    //public bool Search(Point2D in_toFind, ref int foundPos, List<Point2D> in_list)
    //{
    //    int pos = 0;
    //    while (pos < in_list.Count)
    //    {
    //        if (in_toFind.IsEqualTo(in_list[pos]))
    //        {
    //            return true;
    //            foundPos = pos;
    //            break;
    //        }
    //        pos++;
    //    }
    //    return false;
    //}

    public bool BinarySearch(Point2D in_toFind, ref int foundPos, List<Point2D> in_list)
    {
        bool toRet = false;
        int count = 0;
        int pos = RoundedHalf(in_list.Count);
        int delta = pos;
        int pos1 = pos;
        int pos2 = -int.MaxValue;
        int pos3 = -int.MaxValue;

        while ((pos < in_list.Count) && (pos >= 0) && (!PointFound(in_toFind, in_list[pos])))
        {
            //Console.Write(count + ":");
            //Console.Write(pos + ", ");
            Point2D toCompare = in_list[pos];
            if (PointHigher(in_toFind, toCompare))
            {
                pos = pos + delta;
                if (pos >= in_list.Count)
                    pos = in_list.Count - 1;
            }
            else
            {
                pos = pos - delta;
                if (pos < 0)
                    pos = 0;
            }
            delta = RoundedHalf(delta);
            if (delta < 1)
                delta = 1;
            pos2 = pos1;
            pos3 = pos2;
            pos1 = pos;
            if ((pos == pos2) || (pos == pos3))
            {
                //Console.Write("Breaking out...");
                break;
            }
            if (count > (2 * Math.Log(in_list.Count)))
            {
                //Console.Write("Exceeding count... " + (2 * Math.Log(in_list.Count)));
                break;
            }
            else
            {
                //Console.Write("Continuing...");
            }
            count++;
        }
        if ((pos >= 0) && (pos < in_list.Count))
        {
            if (PointFound(in_toFind, in_list[pos]))
            {
                foundPos = pos;
                toRet = true;
            }
        }
        //Console.WriteLine();

        return toRet;
    }

    protected int RoundedHalf(int toHalve)
    {
        if (toHalve == 1)
            return 0;
        else
        {
            float temp = toHalve;
            int rounder = 0;
            if ((temp % 2.0f) >= 0.5f)
                rounder = 1;
            int toRet = (int)(temp / 2.0f) + rounder;
            return toRet;
        }

    }

    protected bool PointFound(Point2D in_toFind, Point2D fromList)
    {
        bool toRet = false;
        if ((in_toFind.x == fromList.x) && (in_toFind.y == fromList.y))
            toRet = true;
        return toRet;
    }

    protected bool PointHigher(Point2D in_toFind, Point2D fromList)
    {
        bool toRet = false;
        if ((in_toFind.x > fromList.x) || ((in_toFind.x == fromList.x) && (in_toFind.y > fromList.y)))
            toRet = true;
        //Program.OutCoord(in_toFind);
        //Console.Write(">? : " + toRet);
        //Program.OutCoord(fromList);
        //Console.Write(", ");
        return toRet;
    }

    public float CurrentArea()
    {
        float toRet = 0f;
        int pos = 0;
        while (pos < interior.Count)
        {
            toRet += areaFunk.CalcArea(interior[pos].y);
            pos++;
        }
        return toRet;
    }

    public Point2D CurrentCenter()
    {
        float xCenter = 0f;
        float yCenter = 0f;
        int pos = 0;
        while (pos < interior.Count)
        {
            Point2D loc = interior[pos];
            xCenter += loc.x;
            yCenter += loc.y;
            pos++;
        }

        xCenter /= pos;
        yCenter /= pos;
        Point2D toRet = new Point2D(RoundedFloatToInt(xCenter), RoundedFloatToInt(yCenter));
        return toRet;
    }

    private int RoundedFloatToInt(float ins)
    {
        int casted = (int)ins;
        ins -= casted;
        if (ins >= 0.5)
            return casted + 1;
        else
            return casted;
    }
}

