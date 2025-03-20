using System.Collections;
using System.Collections.Generic;

public class WorldNode
{
    DrainageVector drainage;
    // 8/23/24
    // Geology deprecated because this info now exists in the resource system
    // May re-implement layer age and the like at some later point but isn't important atm
    //MineralDeposit deposit;
    //NodeGeology geology;
    // needs biomes and climate, these may need to be persistent after generation

    char elevType;
    char terrType;

    float elev;
    float layerElev;
    float meshRadius;

    // simple climate stufff, will be removed later
    float precipitation;
    float meanHighTemperature;
    float meanLowTemperature;
    float oceanSurfaceTemperature;

    public WorldNode()
    {
        Set_drainage(null);
        //Set_deposit(null);
        //Set_geology(null);
        Set_elevType('0');
        Set_terrType('0');
        Set_elev(0.0f);
        Set_layerElev(0.0f);
        Set_meshRadius(0.0f);
        Set_precipitation(0.0f);
    }

    public WorldNode(float in_elev)
    {
        Set_drainage(null);
        //Set_deposit(null);
        //Set_geology(null);
        Set_elevType('0');
        Set_terrType('0');
        Set_elev(in_elev);
        Set_layerElev(0.0f);
        Set_meshRadius(0.0f);
        Set_precipitation(0.0f);
    }

    public WorldNode(float in_elev, float in_layerElev, char in_elevType)
    {
        Set_drainage(null);
        //Set_deposit(null);
        //Set_geology(null);
        Set_elevType(in_elevType);
        Set_terrType('0');
        Set_elev(in_elev);
        Set_layerElev(in_layerElev);
        Set_meshRadius(0.0f);
        Set_precipitation(0.0f);
    }

    public void Set_drainage(DrainageVector ins) { drainage = ins; }
    //public void Set_deposit(MineralDeposit ins) { deposit = ins; }
    //public void Set_geology(NodeGeology ins) { geology = ins; }
    public void Set_elevType(char ins) { elevType = ins; }
    public void Set_terrType(char ins) { terrType = ins; }
    public void Set_elev(float ins) { elev = ins; }
    public void Set_layerElev(float ins) { layerElev = ins; }
    public void Set_meshRadius(float ins) { meshRadius = ins; }
    public void Set_precipitation(float ins) { precipitation = ins; }
    public void Set_meanHighTemperature(float ins) { meanHighTemperature = ins; }
    public void Set_meanLowTemperature(float ins) { meanLowTemperature = ins; }
    public void Set_oceanSurfaceTemperature(float ins) { oceanSurfaceTemperature = ins; }

    public DrainageVector Get_drainage() { return drainage; }
    //public MineralDeposit Get_deposit() { return deposit; }
    //public NodeGeology Get_geology() { return geology; }
    public char Get_elevType() { return elevType; }
    public char Get_terrType() { return terrType; }
    public float Get_elev() { return elev; }
    public float Get_layerElev() { return layerElev; }
    public float Get_meshRadius() { return meshRadius; }
    public float Get_precipitation() { return precipitation; }
    public float Get_meanHighTemperature() { return meanHighTemperature; }
    public float Get_meanLowTemperature() { return meanLowTemperature; }
    public float Get_oceanSurfaceTemperature() { return oceanSurfaceTemperature; }

    public bool IsLand()
    {
        bool toRet = true;
        if ((elevType == 'D') || (elevType == 'B') || (elevType == 'S'))
            toRet = false;
        return toRet;
    }
}


