using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct Corner
{
    public bool nw;
    public bool ne;
    public bool se;
    public bool sw;

    public Corner(bool in_nw, bool in_ne, bool in_se, bool in_sw)
    {
        nw = in_nw;
        ne = in_ne;
        se = in_se;
        sw = in_sw;
    }

}


