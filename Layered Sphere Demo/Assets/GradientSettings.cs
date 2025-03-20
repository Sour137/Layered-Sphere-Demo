using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GradientSettings
{
    public Color colorA = Color.clear;
    float timeA = -1;
    bool setA = false;

    public Color colorB = Color.clear;
    float timeB = -1;
    bool setB = false;

    public Color colorC = Color.clear;
    float timeC = -1;
    bool setC = false;

    public Color colorD = Color.clear;
    float timeD = -1;
    bool setD = false;

    public Color colorE = Color.clear;
    float timeE = -1;
    bool setE = false;

    public Color colorF = Color.clear;
    float timeF = -1;
    bool setF = false;

    public Color colorG = Color.clear;
    float timeG = -1;
    bool setG = false;

    public Color colorH = Color.clear;
    float timeH = -1;
    bool setH = false;

    public GradientColorKey a; // Deep seafloor
    public GradientColorKey b; // Medium Seafloor
    public GradientColorKey c; // Shallow Seafloor
    public GradientColorKey d; // Beach
    public GradientColorKey e; // Lowlands
    public GradientColorKey f; // Medium Lands
    public GradientColorKey g; // High Lands
    public GradientColorKey h; // Snowcaps

    public void BuildKeys()
    {
        if (timeA >= 0)
            setA = true;
        a = new GradientColorKey(colorA, timeA);

        if (timeB >= 0)
            setB = true;
        b = new GradientColorKey(colorB, timeB);

        if (timeC >= 0)
            setC = true;
        c = new GradientColorKey(colorC, timeC);

        if (timeD >= 0)
            setD = true;
        d = new GradientColorKey(colorD, timeD);

        if (timeE >= 0)
            setE = true;
        e = new GradientColorKey(colorE, timeE);

        if (timeF >= 0)
            setF = true;
        f = new GradientColorKey(colorF, timeF);

        if (timeG >= 0)
            setG = true;
        g = new GradientColorKey(colorG, timeG);

        if (timeH >= 0)
            setH = true;
        h = new GradientColorKey(colorH, timeH);


    }

    public bool SetA() { return setA; }
    public bool SetB() { return setB; }
    public bool SetC() { return setC; }
    public bool SetD() { return setD; }
    public bool SetE() { return setE; }
    public bool SetF() { return setF; }
    public bool SetG() { return setG; }
    public bool SetH() { return setH; }

    public void Set_timeA(float ins) { timeA = ins; }
    public void Set_timeB(float ins) { timeB = ins; }
    public void Set_timeC(float ins) { timeC = ins; }
    public void Set_timeD(float ins) { timeD = ins; }
    public void Set_timeE(float ins) { timeE = ins; }
    public void Set_timeF(float ins) { timeF = ins; }
    public void Set_timeG(float ins) { timeG = ins; }
    public void Set_timeH(float ins) { timeH = ins; }

    public float Get_timeA() { return timeA; }
    public float Get_timeB() { return timeB; }
    public float Get_timeC() { return timeC; }
    public float Get_timeD() { return timeD; }
    public float Get_timeE() { return timeE; }
    public float Get_timeF() { return timeF; }
    public float Get_timeG() { return timeG; }
    public float Get_timeH() { return timeH; }


}