using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class WorldAnalyzer
{
    bool[] analysis;
    float NEWWORLD_UPPER = 0.80f;
    float NEWWORLD_LOWER = 0.40f;
    int SIZE_AUSTRALIA = 7600000;
    int SIZE_AMERICAS = 44000000;
    int SIZE_EURASIA = 55000000;
    int TYPECOUNT = 7;

    public WorldAnalyzer(World in_world)
    {
        analysis = Process(in_world);
    }

    private bool[] Process(World in_world)
    {
        bool[] toRet = new bool[TYPECOUNT];
        toRet[0] = Analyze_NewWorld(in_world);
        toRet[1] = Analyze_Sisters(in_world);
        toRet[2] = Analyze_Continents(in_world);
        toRet[3] = Analyze_Arhipelago(in_world);
        toRet[4] = Analyze_Pangaea(in_world);
        toRet[5] = Analyze_Atolls(in_world);
        toRet[6] = Analyze_NoType(toRet);
        return toRet;
    }

    private bool Analyze_NewWorld(World in_world)
    {
        bool toRet = false;
        //AmalgamationSpace.TechBarrierCell[] techCells = in_grid.Get_techBodies();
        BiogeographicZone[] ecoZones = in_world.Get_biogeologicZones().Get_solids(in_world.Get_grid());
        //int lPos = Find_Largest(ecoZones, -1);
        //int nPos = Find_Largest(ecoZones, lPos);
        int lPos = 0; // Presumes sorting
        int nPos = 1;
        if (nPos != -1)
        {
            float lArea = ecoZones[lPos].Get_surfaceArea();
            float nArea = ecoZones[nPos].Get_surfaceArea();
            float ratio = nArea / lArea;
            //Debug.Log("New/Old Ratio = " + ratio);
            if ((ratio >= NEWWORLD_LOWER) && (ratio <= NEWWORLD_UPPER))
                toRet = true;
        }
        return toRet;
    }

    private bool Analyze_Sisters(World in_world)
    {
        bool toRet = false;
        //AmalgamationSpace.TechBarrierCell[] techCells = in_grid.Get_techBodies();
        BiogeographicZone[] ecoZones = in_world.Get_biogeologicZones().Get_solids(in_world.Get_grid());

        int lPos = 0;
        int nPos = 1;
        if (nPos != -1)
        {
            float lArea = ecoZones[lPos].Get_surfaceArea();
            float nArea = ecoZones[nPos].Get_surfaceArea();
            float ratio = nArea / lArea;
            if (ratio > NEWWORLD_UPPER)
                toRet = true;
        }

        return toRet;
    }

    private bool Analyze_Continents(World in_world)
    {
        bool toRet = false;
        //AmalgamationSpace.TechBarrierCell[] techCells = in_grid.Get_techBodies();
        BiogeographicZone[] ecoZones = in_world.Get_biogeologicZones().Get_solids(in_world.Get_grid());

        int lPos = 0;
        float lArea = ecoZones[lPos].Get_surfaceArea();
        if ((lArea >= SIZE_AMERICAS) && (lArea <= SIZE_EURASIA))
            toRet = true;
        return toRet;
    }

    private bool Analyze_Arhipelago(World in_world)
    {
        bool toRet = false;
        //AmalgamationSpace.TechBarrierCell[] techCells = in_grid.Get_techBodies();
        BiogeographicZone[] ecoZones = in_world.Get_biogeologicZones().Get_solids(in_world.Get_grid());

        int lPos = 0;
        float lArea = ecoZones[lPos].Get_surfaceArea();
        if ((lArea >= SIZE_AUSTRALIA) && (lArea <= SIZE_AMERICAS))
            toRet = true;
        return toRet;
    }

    private bool Analyze_Pangaea(World in_world)
    {
        bool toRet = false;
        //AmalgamationSpace.TechBarrierCell[] techCells = in_grid.Get_techBodies();
        BiogeographicZone[] ecoZones = in_world.Get_biogeologicZones().Get_solids(in_world.Get_grid());

        int lPos = 0;
        float lArea = ecoZones[lPos].Get_surfaceArea();
        if (lArea > (SIZE_EURASIA))
            toRet = true;
        return toRet;
    }

    private bool Analyze_Atolls(World in_world)
    {
        bool toRet = false;
        //AmalgamationSpace.TechBarrierCell[] techCells = in_grid.Get_techBodies();
        BiogeographicZone[] ecoZones = in_world.Get_biogeologicZones().Get_solids(in_world.Get_grid());

        int lPos = 0;
        float lArea = ecoZones[lPos].Get_surfaceArea();
        if (lArea < (SIZE_AUSTRALIA))
            toRet = true;
        return toRet;
    }

    private bool Analyze_NoType(bool[] analysis)
    {
        bool toRet = false;
        int pos = 0;
        int lim = analysis.Length;
        while (pos < lim)
        {
            if (analysis[pos])
                break;
            else
                pos++;
            if ((pos == lim) && (!toRet))
                toRet = true;
        }
        return toRet;
    }

    //private int Find_Largest(BiogeographicZone[] in_cells, int in_skip)
    //{
    //    int pos = 0;
    //    int lim = in_cells.Length;
    //    int toRet = -1;
    //    float largestArea = 0.0f;

    //    while (pos < lim)
    //    {
    //        float area = in_cells[pos].Get_surfaceArea();
    //        if (area > largestArea)
    //        {
    //            toRet = pos;
    //            largestArea = area;
    //        }
    //        pos++;
    //        if (pos == in_skip)
    //            pos++;
    //    }

    //    return toRet;
    //}

    public bool[] Get_analysis() { return analysis; }
}

