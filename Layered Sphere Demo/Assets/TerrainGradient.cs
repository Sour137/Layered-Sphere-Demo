using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainGradient
{
    public Gradient terrainGradient;
    public GradientSettings settings;

    public void BuildGradient()
    {
        settings.BuildKeys();
        List<GradientColorKey> keys = new List<GradientColorKey>();
        if (settings.SetA())
            keys.Add(settings.a);
        if (settings.SetB())
            keys.Add(settings.b);
        if (settings.SetC())
            keys.Add(settings.c);
        if (settings.SetD())
            keys.Add(settings.d);
        if (settings.SetE())
            keys.Add(settings.e);
        if (settings.SetF())
            keys.Add(settings.f);
        if (settings.SetG())
            keys.Add(settings.g);
        if (settings.SetH())
            keys.Add(settings.h);
        GradientAlphaKey forwardAlpha = new GradientAlphaKey(1, 0);
        GradientAlphaKey backwardAlpha = new GradientAlphaKey(1, 1);
        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = forwardAlpha;
        alphas[1] = backwardAlpha;
        terrainGradient = new Gradient();
        //terrainGradient.mode = GradientMode.Fixed;
        terrainGradient.SetKeys(keys.ToArray(), alphas);

    }

    public void CalculateGradient(World in_world)
    {
        int pos = 0;
        char[] elevationTypes = in_world.Get_grid().Get_elevationTypes();
        int lim = elevationTypes.Length;
        char current = '0';
        int count = 0;
        int lastCount = 0;
        int lastPos = 0;
        settings.Set_timeA(0.0f);
        settings.Set_timeH(1.0f);
        while (pos < lim)
        {
            char next = elevationTypes[pos];
            if ((next != current) || ((pos + 1) == lim))
            {
                //count /= 2;
                float time = ((float)((count / 2) + lastPos) / ((float)lim));
                switch (current)
                {
                    case 'B':
                        settings.Set_timeB(time * 0.75f);
                        break;
                    case 'S':
                        settings.Set_timeC((float)(lastPos - (lastCount / 2)) / ((float)lim));
                        break;
                    case 'L':
                        settings.Set_timeD(/*0.95f **/ (((float)lastPos - (count)) / (float)lim));
                        settings.Set_timeE(time);
                        break;
                    case 'M':
                        settings.Set_timeF(time);
                        break;
                    case 'H':
                        settings.Set_timeG(time);
                        break;
                    default:
                        break;
                }
                current = next;
                lastCount = count;
                count = 0;
                lastPos = pos;
            }
            else
            {
                count++;
            }
            pos++;
        }
    }
}
