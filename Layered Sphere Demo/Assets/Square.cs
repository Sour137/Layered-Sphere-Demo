using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Square
{
    Vector3 topLeft;
    Vector3 topRight;
    Vector3 botRight;
    Vector3 botLeft;

    int config = 0;

    public Square(Vector3 in_a, Vector3 in_b, Vector3 in_c, Vector3 in_d)
    {
        topLeft = in_a;
        topRight = in_b;
        botRight = in_c;
        botLeft = in_d;
    }

    public int Get_configurationThisElev(float in_elev)
    {
        int configuration = 0;
        if (NodeActive(in_elev, topLeft))
            configuration += 8;
        if (NodeActive(in_elev, topRight))
            configuration += 4;
        if (NodeActive(in_elev, botRight))
            configuration += 2;
        if (NodeActive(in_elev, botLeft))
            configuration += 1;

        if ((configuration == 15) && ((NodeOccluded(in_elev, topLeft)) && (NodeOccluded(in_elev, topRight)) && (NodeOccluded(in_elev, botRight)) && (NodeOccluded(in_elev, botLeft))))
            configuration = 0;
        return configuration;
    }

    //public void LockConfiguration()
    //{
    //    config = Get_configurationThisElev()
    //}

    private bool NodeActive(float in_elev, Vector3 in_node)
    {
        bool toRet = false;
        if (in_node.y >= in_elev)
            toRet = true;
        return toRet;
    }

    private bool NodeOccluded(float in_elev, Vector3 in_node)
    {
        bool toRet = false;
        if (in_node.y > in_elev)
            toRet = true;
        return toRet;
    }

    public Vector3 Get_topLeft() { return topLeft; }
    public Vector3 Get_topRight() { return topRight; }
    public Vector3 Get_botRight() { return botRight; }
    public Vector3 Get_botLeft() { return botLeft; }
}

