using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSubGrid
{
    MinMax mmLayer;
    List<float> layerElevs;
    List<float> layerRadii;
    //EditorSettings settings;

    float highest;
    //float layers;
    //float elevScale;
    //float radius;
    float halfRange;

    //public WorldGrid Generate(WorldGrid in_grid, WorldSettings in_settings)
    //{
    //    WorldGrid toRet = in_grid;
    //    settings = in_settings;
    //    Init(toRet);
    //    int x = 0;
    //    int maxX = toRet.Get_maxX();
    //    int y = 0;
    //    int maxY = toRet.Get_maxY();
    //    while(y < maxY)
    //    {
    //        while(x + 1 < maxX)
    //        {
    //            float[,] lerpedElevs = Expand(x, y, toRet);
    //            float[,] layerdElevs = Stratify(lerpedElevs);
    //            //float[,] layerRadius = Radialize(layerdElevs);
    //            toRet.Get_tile(x, y).Set_subGrids(BuildSubGrids(lerpedElevs, layerdElevs));
    //            x++;
    //        }
    //        x = 0;
    //        y++;
    //    }
    //    toRet = GenerateRadials(toRet);
    //    toRet.Set_layerElevMinMax(mmLayer);
    //    toRet.Set_actualLayers(SortList(layerElevs));
    //    toRet.Set_actualRadii(SortList(layerRadii));
    //    return toRet;
    //}

    public WorldGrid LerpNodes(WorldGrid in_grid, GameData in_data)
    {
        WorldGrid toRet = in_grid;
        WorldNode[,] nodes = toRet.Get_Nodes();
        PointValidator localValidator = new PointValidator(nodes);
        //settings = in_settings;
        Init(toRet);
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 0;
        int maxY = nodes.GetLength(1);

        int lerpX = 0;
        int lerpMaxX = (maxX) * GameSettings.EXPANSION;
        int lerpY = 0;
        int lerpMaxY = (maxY) * GameSettings.EXPANSION;
        int nextY = 0;
        WorldNode[,] lerpNodes = new WorldNode[lerpMaxX, lerpMaxY];
        while (y < maxY)
        {
            while (x < maxX)
            {
                int loopX = lerpX;
                int loopY = lerpY;
                int expandX = 0;
                int expandY = 0;
                float[,] lerpedElevs = Expand(x, y, nodes);
                float[,] layerdElevs = Stratify(lerpedElevs, in_data);
                while (expandY < GameSettings.EXPANSION)
                {
                    lerpX = loopX;
                    while (expandX < GameSettings.EXPANSION)
                    {
                        lerpNodes[lerpX, lerpY] = new WorldNode(lerpedElevs[expandX, expandY], layerdElevs[expandX, expandY], 'E');
                        lerpX++;
                        expandX++;
                    }
                    lerpY++;
                    expandX = 0;
                    expandY++;
                }
                if (nextY != lerpY)
                    nextY = lerpY;
                lerpY = loopY;
                x++;
            }
            lerpY = nextY;
            lerpX = 0;
            x = 0;
            y++;
        }
        toRet.Set_nodes(GenerateNodeRadials(lerpNodes, in_data));
        //toRet.Set_nodes(lerpNodes);
        toRet.Set_layerElevMinMax(mmLayer);
        toRet.Set_actualLayers(SortList(layerElevs));
        toRet.Set_actualRadii(SortList(layerRadii));
        return toRet;
    }

    //private WorldGrid GenerateRadials(WorldGrid in_grid)
    //{
    //    WorldGrid toRet = in_grid;
    //    halfRange = (mmLayer.Max - mmLayer.Min) / 2f;
    //    int x = 0;
    //    int maxX = toRet.Get_maxX();
    //    int y = 0;
    //    int maxY = toRet.Get_maxY();
    //    int lim = WorldSettings.EXPANSION;
    //    while (y < maxY)
    //    {
    //        while (x + 1 < maxX)
    //        {
    //            int subX = 0;
    //            int subY = 0;
    //            while(subY < lim)
    //            {
    //                while(subX < lim)
    //                {
    //                    SubGrid[,] subSet = toRet.Get_tile(x, y).Get_subGrids();
    //                    toRet.Get_tile(x, y).Get_subGrids()[subX, subY].Set_meshRadius(Radialize(subSet[subX, subY]));
    //                    subX++;
    //                }
    //                subX = 0;
    //                subY++;
    //            }
    //            x++;

    //        }
    //        x = 0;
    //        y++;
    //    }
    //    return toRet;
    //}

    private WorldNode[,] GenerateNodeRadials(WorldNode[,] in_nodes, GameData in_data)
    {
        WorldNode[,] nodes = in_nodes;
        halfRange = (mmLayer.Max - mmLayer.Min) / 2f;
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 0;
        int maxY = nodes.GetLength(1);
        while (y < maxY)
        {
            while (x < maxX)
            {
                nodes[x, y].Set_meshRadius(Radialize(nodes[x, y], in_data));
                x++;

            }
            x = 0;
            y++;
        }
        return nodes;
    }

    private void Init(WorldGrid in_grid)
    {
        highest = in_grid.Get_highest();
        //layers = in_grid.Get_layers();
        //elevScale = in_grid.Get_elevScale();
        //radius = in_grid.Get_radius();
        mmLayer = new MinMax();
        layerElevs = new List<float>();
        layerRadii = new List<float>();
    }

    private float[] SortList(List<float> in_list)
    {
        int setPos = 0;
        int setLim = in_list.Count;
        float[] toRet = new float[setLim];
        List<float> unsorted = in_list;
        while (setPos < setLim)
        {
            int pos = 0;
            int lim = unsorted.Count;
            float lowest = float.MaxValue;
            int lowPos = -1;
            while (pos < lim)
            {
                if (in_list[pos] < lowest)
                {
                    lowest = in_list[pos];
                    lowPos = pos;
                }
                pos++;
            }
            toRet[setPos] = unsorted[lowPos];
            unsorted.RemoveAt(lowPos);
            setPos++;
        }
        return toRet;
    }

    // ---------- Interpolate Functions ----------
    //private float[,] Expand(int in_x, int in_y, WorldGrid in_grid)
    //{
    //    float[,] toRet = new float[WorldSettings.EXPANSION, WorldSettings.EXPANSION];
    //    Vector3 a = ProvinceToVector(in_x, in_y, in_grid.Get_tile(in_x, in_y));
    //    Vector3 b = ProvinceToVector(in_x + 1, in_y, in_grid.Get_tile(in_x + 1, in_y));
    //    Vector3 c = ProvinceToVector(in_x + 1, in_y + 1, in_grid.Get_tile(in_x + 1, in_y + 1));
    //    Vector3 d = ProvinceToVector(in_x, in_y + 1, in_grid.Get_tile(in_x, in_y + 1));
    //    float increment = 1.0f / 3f;
    //    int xPos = 0;
    //    float expandX = in_x;
    //    float loopX = expandX;
    //    int yPos = 0;
    //    float expandY = in_y;
    //    while (yPos < WorldSettings.EXPANSION)
    //    {
    //        while (xPos < WorldSettings.EXPANSION)
    //        {
    //            Vector3 interpolatedPoint = QuadLerp(a, b, c, d, expandX - in_x, expandY - in_y);
    //            toRet[xPos, yPos] = interpolatedPoint.y;
    //            expandX += increment;
    //            xPos++;
    //        }
    //        expandX = loopX;
    //        expandY += increment;
    //        xPos = 0;
    //        yPos++;
    //    }
    //    return toRet;
    //}

    private float[,] Expand(int in_x, int in_y, WorldNode[,] in_nodes)
    {
        float[,] toRet = new float[GameSettings.EXPANSION, GameSettings.EXPANSION];
        PointValidator localValidator = new PointValidator(in_nodes);
        Point2D aPoint = localValidator.ValidatePoint(in_x, in_y);
        Point2D bPoint = localValidator.ValidatePoint(in_x + 1, in_y);
        Point2D cPoint = localValidator.ValidatePoint(in_x + 1, in_y + 1);
        Point2D dPoint = localValidator.ValidatePoint(in_x, in_y + 1);
        Vector3 a = new Vector3(aPoint.x, in_nodes[aPoint.x, aPoint.y].Get_elev(), aPoint.y);
        Vector3 b = new Vector3(bPoint.x, in_nodes[bPoint.x, bPoint.y].Get_elev(), bPoint.y);
        Vector3 c = new Vector3(cPoint.x, in_nodes[cPoint.x, cPoint.y].Get_elev(), cPoint.y);
        Vector3 d = new Vector3(dPoint.x, in_nodes[dPoint.x, dPoint.y].Get_elev(), dPoint.y);
        float increment = 1.0f / 3f;
        int xPos = 0;
        float expandX = in_x;
        float loopX = expandX;
        int yPos = 0;
        float expandY = in_y;
        while (yPos < GameSettings.EXPANSION)
        {
            while (xPos < GameSettings.EXPANSION)
            {
                Vector3 interpolatedPoint = QuadLerp(a, b, c, d, expandX - in_x, expandY - in_y);
                toRet[xPos, yPos] = interpolatedPoint.y;
                expandX += increment;
                xPos++;
            }
            expandX = loopX;
            expandY += increment;
            xPos = 0;
            yPos++;
        }
        return toRet;
    }

    //private Vector3 ProvinceToVector(float in_x, float in_y, Province in_prov)
    //{
    //    float elev = (float)in_prov.Get_elevation();
    //    return new Vector3(in_x, elev, in_y);
    //}

    private Vector3 QuadLerp(Vector3 in_a, Vector3 in_b, Vector3 in_c, Vector3 in_d, float in_x, float in_y)
    {
        Vector3 abu = Vector3.Lerp(in_a, in_b, in_x);
        Vector3 dcu = Vector3.Lerp(in_d, in_c, in_x);
        return Vector3.Lerp(abu, dcu, in_y);
    }


    // ---------- Layer Functions ----------
    private float[,] Stratify(float[,] in_elevs, GameData in_data)
    {
        float[,] toRet = new float[GameSettings.EXPANSION, GameSettings.EXPANSION];
        int x = 0;
        int y = 0;
        int lim = GameSettings.EXPANSION;
        while (y < lim)
        {
            while (x < lim)
            {
                float locElev = in_elevs[x, y];
                float adjusted = (locElev / highest) * (float)in_data.settings.layers;
                float layered = RoundedFloat(adjusted) / (float)in_data.settings.layers;
                float scaled = layered * in_data.settings.elevScale;
                toRet[x, y] = scaled;
                mmLayer.AddValue(scaled);
                if (UniqueLayer(scaled, layerElevs.ToArray()))
                    layerElevs.Add(scaled);
                x++;
            }
            x = 0;
            y++;
        }
        return toRet;
    }

    private int RoundedFloat(float ins)
    {
        int toRet = (int)ins;
        float remain = ins - toRet;
        if (remain >= 0.5f)
            toRet++;
        return toRet;
    }

    private bool UniqueLayer(float in_elev, float[] in_elevs)
    {
        bool toRet = true;
        int pos = 0;
        int lim = in_elevs.Length;
        while (pos < lim)
        {
            if (in_elev == in_elevs[pos])
            {
                toRet = false;
                break;
            }
            pos++;
        }
        return toRet;
    }


    // ---------- Radius Functions ----------

    private float Radialize(WorldNode in_node, GameData in_data)
    {
        float offset = in_node.Get_layerElev() - (mmLayer.Min + halfRange);
        float locRadius = in_data.settings.radius + offset;
        if (UniqueLayer(locRadius, layerRadii.ToArray()))
            layerRadii.Add(locRadius);
        return locRadius;
    }

}
