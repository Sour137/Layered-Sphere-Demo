using System.Collections;
using System.Collections.Generic;

public class NormalizedFloat
{
    float value;
    public static float maximum;
    public static float minimum;

    public NormalizedFloat()
    {
        value = 0;
        maximum = 1f;
        minimum = 0f;
    }

    public NormalizedFloat(float ins)
    {
        value = ins;
        maximum = 1f;
        minimum = 0f;
    }

    public void Set_value(float ins) { value = ins; }
    public float Get_value() { return ((value - minimum) / maximum); }
}
