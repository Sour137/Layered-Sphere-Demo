//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


//public class Biome
//{
//    char thermal;
//    char precip;
//    char terrain;
//    String name;
//    String mutant;
//    float travelMod;
//    float priceMod;

//    public Biome()
//    {
//        Set_thermal('N');
//        Set_precip('P');
//        Set_terrain('0');
//        Set_name("DEFAULT BIOME");
//        Set_mutant("DEFUALT BIOME MUTANT");
//        Set_travelMod(1.0f);
//        Set_priceMod(1.0f);
//    }

//    public Biome(char in_thermal, char in_precip, char in_terrain, String in_name, String in_mutant, float in_tMod, float in_pMod)
//    {
//        Set_thermal(in_thermal);
//        Set_precip(in_precip);
//        Set_terrain(in_terrain);
//        Set_name(in_name);
//        Set_mutant(in_mutant);
//        Set_priceMod(in_pMod);
//        Set_travelMod(in_tMod);
//    }

//    public char Get_thermal() { return thermal; }
//    public char Get_precip() { return precip; }
//    public char Get_terrain() { return terrain; }
//    public String Get_name() { return name; }
//    public String Get_mutant() { return mutant; }
//    public float Get_travelMod() { return travelMod; }
//    public float Get_priceMod() { return priceMod; }

//    private void Set_thermal(char in_char) { thermal = in_char; }
//    private void Set_precip(char in_char) { precip = in_char; }
//    private void Set_terrain(char in_char) { terrain = in_char; }
//    private void Set_name(String in_name) { name = in_name; }
//    private void Set_mutant(String in_mutant) { mutant = in_mutant; }
//    private void Set_travelMod(float in_value) { travelMod = in_value; }
//    private void Set_priceMod(float in_value) { priceMod = in_value; }
//}

