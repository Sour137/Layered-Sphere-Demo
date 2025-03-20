using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Coast
{
    CoastSegment[] segments;

    public Coast()
    {
        segments = new CoastSegment[0];
    }

    public Coast(CoastSegment[] in_segments)
    {
        Set_segments(in_segments);
    }

    public CoastSegment[] Get_segments() { return segments; }

    public void Set_segments(CoastSegment[] in_segments) { segments = in_segments; }
}

