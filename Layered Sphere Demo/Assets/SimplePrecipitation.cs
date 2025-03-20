using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class SimplePrecipitation
{
    public WorldGrid Generate(WorldGrid in_grid)
    {
        WorldGrid toRet = in_grid;
        WorldNode[,] nodes = in_grid.Get_Nodes();
        int x = 0;
        int y = 0;
        int maxX = nodes.GetLength(0);
        int maxY = nodes.GetLength(1);
        float highest = 0f;
        float lowest = float.MaxValue;
        PrecipitationTrigModel model = new PrecipitationTrigModel(maxY);

        while (y < maxY)
        {
            while (x < maxX)
            {
                //if (nodes[x, y].IsLand())
                //{

                //}
                float score = model.Get_PrecipTrigModelValue(y, maxY);
                float xVariation = GetLocalVariation(x, maxX, 0.5f) / 2f;
                float yVariation = GetLocalVariation(y, maxY, 0.5f) / 2f;
                float modifiedScore = (xVariation + yVariation) * score;

                if (toRet.Get_node(x, y).IsLand())
                {
                    if (modifiedScore > highest)
                        highest = modifiedScore;
                    if (modifiedScore < lowest)
                        lowest = modifiedScore;
                }


                toRet.Get_node(x, y).Set_precipitation(modifiedScore);
                x++;
            }
            x = 0;
            y++;
        }

        Debug.Log("The most precipitation in a node is " + highest + " mm");
        Debug.Log("The least precipitation in a node is " + lowest + " mm");
        return toRet;
    }

    private float GetLocalVariation(float in_pos, float in_lim, float in_a)
    {
        //float inside = (3f * in_pos * MathF.PI) / in_lim;
        //float upper = MathF.Sin(inside) + 2f;
        //float toRet = upper / 2f;
        float inner = (3f * in_pos * MathF.PI) / in_lim;
        float left = (in_a * MathF.Sin(inner)) / 2f;
        float right = 1f - (in_a / 2f);
        float toRet = left + right;
        return toRet;
    }
}




