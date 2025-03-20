using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class WorldNodeType
{
    float oceanElev;
    float oceanArea;
    int oceanCount;
    int oceanPos;

    float[] elevSet;
    float[] elevArea;
    float[] elevMeters;
    int[] elevCounts;
    char[] elevTypes;


    public WorldNodeType()
    {
    }

    public WorldGrid GenerateNodes(WorldGrid in_grid, GameData in_data)
    {
        WorldGrid toRet = in_grid;
        CountElevations(toRet.Get_Nodes());
        CalculateOceanElev(in_data.settings.percentOcean);
        CalculateElevTypes();
        toRet.Set_nodes(ApplyToNodes(toRet.Get_Nodes(), in_data));
        toRet.Set_elevationTypes(elevTypes); // Province to WorldNode Refactor: Should remove
        toRet.Set_oceanPos(oceanPos);
        return toRet;
    }

    private WorldNode[,] ApplyToNodes(WorldNode[,] in_nodes, GameData in_data)
    {
        WorldNode[,] nodes = in_nodes;
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 0;
        int maxY = nodes.GetLength(1);
        while (y < maxY)
        {
            while (x < maxX)
            {
                int thisPos = FindInSet(in_nodes[x, y].Get_layerElev());
                nodes[x, y].Set_elevType(elevTypes[thisPos]);
                nodes[x, y].Set_terrType(CalculateTerrType(x, y, nodes, in_data));
                x++;
            }
            x = 0;
            y++;
        }
        return nodes;
    }

    private char CalculateTerrType(int in_x, int in_y, WorldNode[,] in_nodes, GameData in_data)
    {
        Point2D loc = new Point2D(in_x, in_y);
        Point2D[] peri = loc.GetPerimeter();
        PointValidator validator = new PointValidator(in_nodes);
        int pos = 0;
        int lim = peri.Length;
        float thisMeter = elevMeters[FindInSet(in_nodes[in_x, in_y].Get_layerElev())];
        int flat = 0;
        int hill = 0;
        int mountain = 0;
        while (pos < lim)
        {
            Point2D adjPoint = validator.ValidatePoint(peri[pos]);
            float adjMeter = elevMeters[FindInSet(in_nodes[adjPoint.x, adjPoint.y].Get_layerElev())];
            float delta = Mathf.Abs(thisMeter - adjMeter);
            if (delta <= in_data.settings.deltaFlat)
                flat++;
            else if ((delta > in_data.settings.deltaFlat) && (delta <= in_data.settings.deltaHill))
                hill++;
            else
                mountain++;

            pos++;
        }

        char toRet;
        if ((flat >= hill) && (flat >= mountain))
            toRet = 'F';
        else if ((flat < hill) && (hill >= mountain))
            toRet = 'H';
        else
            toRet = 'M';
        return toRet;
    }

    private int FindInSet(float in_elev)
    {
        int pos = 0;
        int lim = elevSet.Length;
        while (pos < lim)
        {
            if (in_elev == elevSet[pos])
                break;
            else
                pos++;
        }
        return pos;
    }

    private void CalculateElevTypes()
    {
        int pos = 0;
        int lim = elevCounts.Length;
        elevMeters = new float[lim];
        elevTypes = new char[lim];
        bool wet = true;
        float meter = GameSettings.RANGE_OCEAN;
        float increment = GameSettings.RANGE_OCEAN / oceanPos;
        while (pos < lim)
        {
            char type = '0';
            if (wet)
            {
                if (meter >= 0)
                {
                    elevMeters[pos] = meter;
                    if (meter >= GameSettings.BELOW_DEEP)
                        type = 'D';
                    else if (meter >= GameSettings.BELOW_BATH)
                        type = 'B';
                    else
                        type = 'S';
                    meter -= increment;
                }
                else
                {
                    wet = false;
                    meter = 0;
                    increment = GameSettings.RANGE_SURFACE / (lim - oceanPos);
                }
            }
            if ((!wet) && (meter < GameSettings.RANGE_SURFACE))
            {
                elevMeters[pos] = meter;
                if (meter <= GameSettings.ABOVE_MEDIUM)
                    type = 'L';
                else if (meter <= GameSettings.ABOVE_HIGH)
                    type = 'M';
                else
                    type = 'H';
                meter += increment;
            }
            elevTypes[pos] = type;
            pos++;
        }
    }

    private void CalculateOceanElev(float in_percentOcean)
    {
        int pos = 0;
        int lim = elevArea.Length;
        float areaSum = 0.0f;
        int count = 0;
        while (pos < lim)
        {
            areaSum += elevArea[pos];
            count += elevCounts[pos];
            float percent = areaSum / (float)WorldTileArea.EARTHSURFACEAREA;
            if (percent >= in_percentOcean)
                break;
            else
                pos++;
        }
        oceanElev = elevSet[pos];
        oceanArea = areaSum;
        oceanCount = count;
        oceanPos = pos;
    }

    private void CountElevations(WorldNode[,] in_nodes)
    {
        int x = 0;
        int maxX = in_nodes.GetLength(0);
        int y = 0;
        int maxY = in_nodes.GetLength(1);
        WorldTileArea areaFunk = new WorldTileArea(maxX, maxY);
        List<float> elevs = new List<float>();
        List<int> counts = new List<int>();
        List<float> areaAtElev = new List<float>();
        while (y < maxY)
        {
            float rowNodeArea = areaFunk.CalcArea(y);
            while (x < maxX)
            {
                float elev = in_nodes[x, y].Get_layerElev();
                int foundPos = 0;
                if (UniqueElev(in_nodes[x, y].Get_layerElev(), elevs, ref foundPos))
                {
                    elevs.Add(elev);
                    counts.Add(1);
                    areaAtElev.Add(rowNodeArea);
                }
                else
                {
                    counts[foundPos]++;
                    areaAtElev[foundPos] += rowNodeArea;
                }
                x++;
            }
            x = 0;
            y++;
        }

        SortSets(elevs, counts, areaAtElev);
    }

    private bool UniqueElev(float in_elev, List<float> in_set, ref int foundPos)
    {
        bool toRet = true;
        int pos = 0;
        int lim = in_set.Count;
        while (pos < lim)
        {
            float thisElev = in_set[pos];
            if (thisElev == in_elev)
            {
                foundPos = pos;
                toRet = false;
                break;
            }
            pos++;
        }
        return toRet;
    }

    private void SortSets(List<float> in_elevSet, List<int> in_countSet, List<float> in_areas)
    {
        int sortPos = 0;
        int lim = in_elevSet.Count;
        List<float> unsortedElevs = in_elevSet;
        List<int> unsortedCounts = in_countSet;
        List<float> unsortedAreas = in_areas;
        elevSet = new float[lim];
        elevCounts = new int[lim];
        elevArea = new float[lim];
        while (sortPos < lim)
        {
            int pos = 0;
            int lowPos = 0;
            float lowElev = float.MaxValue;
            while (pos < unsortedElevs.Count)
            {
                if (unsortedElevs[pos] < lowElev)
                {
                    lowPos = pos;
                    lowElev = unsortedElevs[lowPos];
                }
                pos++;
            }
            elevSet[sortPos] = unsortedElevs[lowPos];
            elevCounts[sortPos] = unsortedCounts[lowPos];
            elevArea[sortPos] = unsortedAreas[lowPos];
            unsortedElevs.RemoveAt(lowPos);
            unsortedCounts.RemoveAt(lowPos);
            unsortedAreas.RemoveAt(lowPos);
            sortPos++;
        }

    }

    public float Get_oceanElev() { return oceanElev; }
}

