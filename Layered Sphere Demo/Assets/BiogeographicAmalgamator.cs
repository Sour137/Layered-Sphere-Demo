using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiogeographicAmalgamator : Amalgamator
{
    const float LATITUDE_A = 0.000529f;
    const float LATITUDE_B = 1.05f;
    float minscore;
    float[,] scores;
    List<IterableBinaryBubbleCell> realms;

    public BiogeographicAmalgamator(GameData in_data)
    {
        minscore = in_data.settings.biogeographicScore;
    }

    public WorldGrid Generate(WorldGrid in_grid)
    {
        WorldGrid toRet = in_grid;
        scores = CalculateScores(toRet);
        List<IterableBinaryBubbleCell> solids = NodeAmalgamate(toRet, false);
        realms = SortCells(solids);

        return toRet;
    }

    protected override bool NodeValid(WorldNode[,] in_node, Point2D in_point)
    {
        bool toRet = false;
        if (scores[in_point.x, in_point.y] >= minscore)
            toRet = true;
        return toRet;
    }

    protected override bool NodeValid(WorldNode[,] in_node, Point2D in_point, IterableBinaryBubbleCell in_cell)
    {
        throw new System.NotImplementedException();
    }

    private float[,] CalculateScores(WorldGrid in_grid)
    {
        WorldNode[,] nodes = in_grid.Get_Nodes();
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 0;
        int maxY = nodes.GetLength(1);
        float[,] toRet = new float[maxX, maxY];
        while (y < maxY)
        {
            while (x < maxX)
            {
                toRet[x, y] = CalculateScore(nodes[x, y], y, maxY);
                x++;
            }
            x = 0;
            y++;
        }
        return toRet;
    }

    private float CalculateScore(WorldNode in_node, float in_y, float in_maxY)
    {
        float elevScore = CalculateElevScore(in_node.Get_elevType());
        float latScore = CalculateLatitudeScore(in_y, in_maxY);
        float toRet = elevScore * latScore;

        return toRet;
    }

    private float CalculateElevScore(char in_type)
    {
        float toRet;
        switch (in_type)
        {
            case 'H':
            case 'M':
            case 'L':
                toRet = 1f;
                break;
            case 'D':
                toRet = 0f;
                break;
            case 'B':
                toRet = 1f / 3f;
                break;
            case 'S':
                toRet = 2f / 3f;
                break;
            default:
                toRet = 0f;
                break;
        }
        return toRet;
    }

    private float CalculateLatitudeScore(float in_y, float in_maxY)
    {
        // Probably replace with a biome score when we get there...
        float a = CalculateLatitudeScoreComponent(in_y, in_maxY, 0f);
        float b = CalculateLatitudeScoreComponent(in_y, in_maxY, 67f);
        float c = CalculateLatitudeScoreComponent(in_y, in_maxY, 113f);
        float d = CalculateLatitudeScoreComponent(in_y, in_maxY, 180f);
        float toRet = 1 - (a + b + c + d);
        return toRet;
    }

    private float CalculateLatitudeScoreComponent(float in_y, float in_maxY, float in_latComponent)
    {
        float lat = in_y / in_maxY;
        float expo = lat - (in_latComponent / 180f);
        float right = Mathf.Pow(expo, 2);
        float inner = LATITUDE_A + right;
        float denom = LATITUDE_B * inner;
        float toRet = LATITUDE_A / denom;

        return toRet;
    }

    private BiogeographicZone[] SolidifyAll(List<IterableBinaryBubbleCell> in_cells, WorldGrid in_grid)
    {
        int pos = 0;
        int lim = in_cells.Count;
        //BiogeographicZone[] toRet = new BiogeographicZone[lim];
        List<BiogeographicZone> zones = new List<BiogeographicZone>();
        while (pos < lim)
        {
            zones.Add(new BiogeographicZone(in_cells[pos], in_grid));
            pos++;
        }
        //return SortIt(zones); // broken ?? Fixed
        return zones.ToArray();
    }

    //private BiogeographicZone[] SortIt(List<BiogeographicZone> in_solids)
    //{
    //    BiogeographicZone[] toRet = new BiogeographicZone[in_solids.Count];
    //    List<BiogeographicZone> unsorted = in_solids;
    //    int sortPos = 0;
    //    while (sortPos < toRet.Length)
    //    {
    //        int pos = 0;
    //        int highPos = 0;
    //        float largest = 0f;
    //        while (highPos < unsorted.Count)
    //        {
    //            if (unsorted[pos].Get_surfaceArea() > largest)
    //            {
    //                highPos = pos;
    //                largest = unsorted[pos].Get_surfaceArea();
    //            }
    //            pos++;
    //        }
    //        toRet[sortPos] = unsorted[highPos];
    //        unsorted.RemoveAt(highPos);
    //        sortPos++;
    //    }
    //    return toRet;
    //}

    //private List<IterableBinaryBubbleCell> SortCells(List<IterableBinaryBubbleCell> in_cells)
    //{
    //    List<IterableBinaryBubbleCell> toRet = new List<IterableBinaryBubbleCell>();
    //    List<IterableBinaryBubbleCell> unsorted = in_cells;
    //    int sortPos = 0;
    //    while (sortPos < in_cells.Count)
    //    {
    //        int pos = 0;
    //        int highPos = 0;
    //        float largest = 0f;
    //        while (pos < unsorted.Count)
    //        {
    //            if (unsorted[pos].CurrentArea() > largest)
    //            {
    //                highPos = pos;
    //                largest = unsorted[pos].CurrentArea();
    //            }
    //            pos++;
    //        }
    //        toRet.Add(unsorted[highPos]);
    //        unsorted.RemoveAt(highPos);
    //        sortPos++;
    //    }
    //    return toRet;
    //}

    private float GetLargest(BiogeographicZone[] in_solids)
    {
        float toRet = 0.0f;
        int pos = 0;
        int lim = in_solids.Length;
        while (pos < lim)
        {
            if (toRet < in_solids[pos].Get_surfaceArea())
                toRet = in_solids[pos].Get_surfaceArea();
            pos++;
        }

        return toRet;
    }

    public BiogeographicZone[] Get_solids(WorldGrid in_grid) { return SolidifyAll(realms, in_grid); }
    public List<IterableBinaryBubbleCell> Get_realms() { return realms; }
    public float[,] Get_scores() { return scores; }


}


