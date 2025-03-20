using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTemperature
{
    public WorldGrid Generate(WorldGrid in_grid, GameData in_data)
    {
        WorldGrid toRet = in_grid;
        WorldNode[,] nodes = in_grid.Get_Nodes();
        int x = 0;
        int y = 0;
        int maxX = nodes.GetLength(0);
        int maxY = nodes.GetLength(1);

        while (y < maxY)
        {
            while (x < maxX)
            {
                float latTemp = LatitudeTemperature(y, maxY, in_data);
                float elevOffset = ElevationOffset(x, y, toRet);
                float seasonalOffset = SeasonalOffset(y, maxY);
                float meanHigh = latTemp - elevOffset;
                float meanLow = meanHigh - seasonalOffset;
                toRet.Get_node(x, y).Set_meanHighTemperature(meanHigh);
                toRet.Get_node(x, y).Set_meanLowTemperature(meanLow);

                float oceanSurfaceTemperature = CalculateOceanSurfaceTemperature(y, maxY, in_data);
                toRet.Get_node(x, y).Set_oceanSurfaceTemperature(oceanSurfaceTemperature);

                x++;
            }
            x = 0;
            y++;
        }
        return toRet;
    }

    private float LatitudeTemperature(float in_y, float in_maxY, GameData in_data)
    {
        //float offset = in_maxY / 2f;
        //float inner = in_y - offset;
        //float square = Mathf.Pow(inner, 2f);
        //float factor = 0.225f / (offset * 11f);
        //float denom = square * factor + 1;
        //float left = 38f / denom;
        //float toRet = left - 10f;

        // cast to 90 degrees
        float normalized = in_y / in_maxY;
        float offset = 0.5f;
        float latitude = (180f - in_data.settings.polarBuffer) * (normalized - offset);
        // then the function
        float square = MathF.Pow(latitude, 2);
        float denom = 0.0005f * square + 1;
        float left = 38f / denom;
        float toRet = left - 10f;

        return toRet;
    }

    private float ElevationOffset(int in_x, int in_y, WorldGrid in_grid)
    {
        float ratio = CalculateRatio(in_grid.Get_node(in_x, in_y).Get_layerElev(), in_grid);
        float maxElevation = GameSettings.ABOVE_HIGH;
        float projectedElevation = maxElevation * ratio;
        float count = projectedElevation / 1000f;
        float toRet = 7.9f * count;
        return toRet;
    }

    private float SeasonalOffset(float in_y, float in_maxY)
    {
        float halfOffset = in_maxY / 2f;
        float upperInner = in_y - halfOffset;
        float upper = -1f * Mathf.Pow(upperInner, 2);
        float lower = 0.246f * Mathf.Pow(halfOffset, 2);
        float exponent = upper / lower;
        float rightSide = 35 * Mathf.Exp(exponent);
        float toRet = 35 - rightSide;
        return toRet;
    }

    private float CalculateRatio(float in_layer, WorldGrid in_grid)
    {
        int layerPos = FindInSet(in_layer, in_grid.Get_actualLayers());
        int oceanPos = in_grid.Get_oceanPos();
        if (layerPos <= oceanPos)
            layerPos = oceanPos + 0;
        //else
        //{
        //    int max = in_grid.Get_actualLayers().Length;
        //    float upper = layerPos - oceanPos;
        //    float lower = max - oceanPos;
        //    return upper / lower;
        //}
        int max = in_grid.Get_actualLayers().Length;
        float upper = layerPos - oceanPos;
        float lower = max - oceanPos;
        return upper / lower;
    }

    private int FindInSet(float in_toFind, float[] in_set)
    {
        int pos = 0;
        while (pos < in_set.Length)
        {
            if (in_toFind == in_set[pos])
                return pos;
            pos++;
        }
        return -1;
    }

    private float CalculateOceanSurfaceTemperature(float in_y, float in_maxY, GameData in_data)
    {
        float normalized = in_y / in_maxY;
        float offset = 0.5f;
        float latitude = (180f - in_data.settings.polarBuffer) * (normalized - offset);

        float square = MathF.Pow(latitude, 2);
        float denom = 0.0005f * square + 1;
        float left = 42f / denom;
        float toRet = left - 10f;

        return toRet;
    }
}

