using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ColorSettings : ScriptableObject
{
    //public Gradient terrainGradient;
    public Material terrainMaterial;
    public Material OceanMaterial;
    public TerrainGradient[] terrainGradients;
    public OceanGradient oceanGradient;
    float oceanRadius;

    public float Get_oceanRadius() { return oceanRadius; }
    public void Set_oceanRadius(float ins) { oceanRadius = ins; }
}