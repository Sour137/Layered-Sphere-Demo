using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OceanGradient
{
    public Gradient gradient;
    public GradientSettings settings;
    [Range(0f, 1f)]
    public float shallowAlpha = 1.0f;

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
        GradientAlphaKey backwardAlpha = new GradientAlphaKey(1, settings.Get_timeB());
        GradientAlphaKey shallow = new GradientAlphaKey(shallowAlpha, 1);
        GradientAlphaKey[] alphas = new GradientAlphaKey[3];
        alphas[0] = forwardAlpha;
        alphas[1] = backwardAlpha;
        alphas[2] = shallow;
        gradient = new Gradient();
        //OceanGradient.mode = GradientMode.Fixed;
        gradient.SetKeys(keys.ToArray(), alphas);

    }

    public void CalculateGradient(World in_world)
    {
        int pos = 0;
        char[] elevationTypes = in_world.Get_grid().Get_elevationTypes();
        int lim = elevationTypes.Length;
        char current = elevationTypes[0];
        int count = 0;
        int lastPos = 0;
        settings.Set_timeA(0.0f);
        settings.Set_timeC(1.0f);
        while (pos < lim)
        {
            char next = elevationTypes[pos];
            if (current != next)
            {
                if (next == 'B')
                {
                    current = 'B';
                    count = 0;
                    lastPos = pos;
                }
                else if (next == 'S')
                {
                    float time = (((float)(count + lastPos)) / (2.0f * lim));
                    settings.Set_timeB((float)pos / (float)lim);
                    break;
                }
            }
            pos++;
        }
    }
}
