using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class River
{
    RiverSegment[] segments;
    float totVolume;

    public River()
    {
        segments = new RiverSegment[0];
        Set_totVolume(0.0f);
    }

    public River(RiverSegment[] in_segments, float in_totVolume)
    {
        Set_segments(in_segments);
        Set_totVolume(in_totVolume);
    }

    public RiverSegment[] Get_segments() { return segments; }
    public float Get_totVolume() { return totVolume; }

    public void Set_segments(RiverSegment[] in_segments) { segments = in_segments; }
    public void Set_totVolume(float in_totV) { totVolume = in_totV; }
}

