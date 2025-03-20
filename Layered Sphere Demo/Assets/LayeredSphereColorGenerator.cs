using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayeredSphereColorGenerator
{
    ColorSettings settings;
    Texture2D landTexture;
    Texture2D regionTexture;
    Texture2D resourceTexture;
    Texture2D oceanTexture;
    const int RESOLUTION = 50;
    float maxRadius = 0.0f;
    float minRadius = 0.0f;
    int regionalShading = 0;


    public LayeredSphereColorGenerator(float in_maxRadius, float in_minRadius)
    {
        maxRadius = in_maxRadius;
        minRadius = in_minRadius;

    }

    public void UpdateColors(ColorSettings in_settings, World in_world)
    {
        settings = in_settings;
        if ((landTexture == null) || (landTexture.height != settings.terrainGradients.Length))
            landTexture = new Texture2D(RESOLUTION, settings.terrainGradients.Length, TextureFormat.RGBA32, false);
        Color[] colors = new Color[landTexture.width * landTexture.height];
        int index = 0;
        foreach (var gradient in settings.terrainGradients)
        {
            gradient.CalculateGradient(in_world);
            gradient.BuildGradient();
            for (int i = 0; i < RESOLUTION; i++)
            {
                colors[index] = gradient.terrainGradient.Evaluate(i / (RESOLUTION - 1f));
                index++;
                //colors[i] = settings.terrainGradient.Evaluate(i / (RESOLUTION - 1f));
            }
        }

        landTexture.SetPixels(colors);
        landTexture.wrapMode = TextureWrapMode.Clamp;
        settings.terrainMaterial.SetTexture("_LandTex", landTexture);
        //settings.terrainMaterial.SetFloat("_maxRadius", maxRadius);
        //settings.terrainMaterial.SetFloat("_minRadius", minRadius);
        landTexture.Apply();

        //UpdateRegionColors(in_world.Get_landBodies().Get_rawCells());
        //ToggleRegionalShading();
        //ToggleRegionalShading();

        if ((oceanTexture == null) || (oceanTexture.height != settings.terrainGradients.Length))
            oceanTexture = new Texture2D(RESOLUTION, 1, TextureFormat.RGBA32, false);
        colors = new Color[oceanTexture.width * oceanTexture.height];
        index = 0;
        settings.oceanGradient.CalculateGradient(in_world);
        settings.oceanGradient.BuildGradient();
        for (int i = 0; i < RESOLUTION; i++)
        {
            colors[index] = settings.oceanGradient.gradient.Evaluate(i / (RESOLUTION - 1f));
            index++;
            //colors[i] = settings.terrainGradient.Evaluate(i / (RESOLUTION - 1f));
        }
        oceanTexture.SetPixels(colors);
        oceanTexture.wrapMode = TextureWrapMode.Clamp;
        //settings.OceanMaterial.SetTexture("_WaterColorGradientTexture", oceanTexture);
        settings.OceanMaterial.SetTexture("_OceanTex", oceanTexture);
        //settings.OceanMaterial.SetFloat("_oceanRadius", settings.oceanRadius);
        //settings.OceanMaterial.SetFloat("_minRadius", minRadius);
        oceanTexture.Apply();
    }

    public void UpdateHeatMapColors()
    {
        if ((regionTexture == null) || (regionTexture.width != 100))
            regionTexture = new Texture2D(100, 1, TextureFormat.RGBA32, false);
        Color red = Color.red;
        Color blue = Color.blue;
        List<Color> heatMapColors = new List<Color>();
        int pos = 0;
        while (pos < 100)
        {
            float loc = (float)pos / 100f;
            heatMapColors.Add(Color.Lerp(blue, red, loc));
            pos++;
        }
        heatMapColors.Add(red);
        regionTexture.SetPixels(heatMapColors.ToArray());
        regionTexture.wrapMode = TextureWrapMode.Clamp;
        //regionTexture.filterMode = FilterMode.Bilinear;
        settings.terrainMaterial.SetTexture("_RegionTex", regionTexture);
        settings.OceanMaterial.SetTexture("_RegionTex", regionTexture);
        regionTexture.Apply();
    }

    public void UpdateRegionColors(List<IterableBinaryBubbleCell> in_cells)
    {
        if ((regionTexture == null) || (regionTexture.width != in_cells.Count + 1))
            regionTexture = new Texture2D(in_cells.Count + 1, 1, TextureFormat.RGBA32, false);
        List<Color> regionColors = new List<Color>();
        regionColors.Add(new Color(0.5f, 0.5f, 0.5f)); // default color
        int pos = 0;
        int lim = in_cells.Count;
        float b = (float)lim;
        float c = 0.75f;
        while (pos < lim)
        {
            //float red = CalculateRed(pos, a, b, c);
            //float green = CalculateGreen(pos, a, b, c);
            //float blue = CalculateBlue(pos, a, b, c);
            float a = CalculateA(pos, b);
            float red = CalculateRed(pos, lim, a);
            float green = CalculateGreen(pos, lim, a);
            float blue = CalculateBlue(pos, lim, a);
            float time = (float)pos / b;
            //GradientColorKey key = new GradientColorKey(new Color(red, green, blue), time);
            Color key = new Color(red, green, blue);
            regionColors.Add(key);
            pos++;
        }
        //GradientAlphaKey forwardAlpha = new GradientAlphaKey(1, 0);
        //GradientAlphaKey backwardAlpha = new GradientAlphaKey(1, 1);
        //GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        //alphas[0] = forwardAlpha;
        //alphas[1] = backwardAlpha;

        //regionColors = ShuffleColors(regionColors);
        regionTexture.SetPixels(regionColors.ToArray());
        regionTexture.wrapMode = TextureWrapMode.Clamp;
        regionTexture.filterMode = FilterMode.Point;
        settings.terrainMaterial.SetTexture("_RegionTex", regionTexture);
        settings.OceanMaterial.SetTexture("_RegionTex", regionTexture);
        //settings.terrainMaterial.SetFloat("_maxRadius", maxRadius);
        //settings.terrainMaterial.SetFloat("_minRadius", minRadius);
        regionTexture.Apply();
    }

    //public void UpdateResourceTexture(List<IterableBinaryBubbleCell> in_cells)
    //{
    //    if ((regionTexture == null) || (regionTexture.width != in_cells.Count + 1))
    //        regionTexture = new Texture2D(in_cells.Count + 1, 1, TextureFormat.RGBA32, false);
    //    List<Color> regionColors = new List<Color>();
    //    regionColors.Add(new Color(0.5f, 0.5f, 0.5f)); // default color
    //    int pos = 0;
    //    int lim = in_cells.Count;
    //    float b = (float)lim;
    //    float c = 0.75f;
    //    while (pos < lim)
    //    {
    //        //float red = CalculateRed(pos, a, b, c);
    //        //float green = CalculateGreen(pos, a, b, c);
    //        //float blue = CalculateBlue(pos, a, b, c);
    //        float a = CalculateA(pos, b);
    //        float red = CalculateRed(pos, lim, a);
    //        float green = CalculateGreen(pos, lim, a);
    //        float blue = CalculateBlue(pos, lim, a);
    //        float time = (float)pos / b;
    //        //GradientColorKey key = new GradientColorKey(new Color(red, green, blue), time);
    //        Color key = new Color(red, green, blue);
    //        regionColors.Add(key);
    //        pos++;
    //    }
    //    //GradientAlphaKey forwardAlpha = new GradientAlphaKey(1, 0);
    //    //GradientAlphaKey backwardAlpha = new GradientAlphaKey(1, 1);
    //    //GradientAlphaKey[] alphas = new GradientAlphaKey[2];
    //    //alphas[0] = forwardAlpha;
    //    //alphas[1] = backwardAlpha;

    //    //regionColors = ShuffleColors(regionColors);
    //    regionTexture.SetPixels(regionColors.ToArray());
    //    regionTexture.wrapMode = TextureWrapMode.Clamp;
    //    regionTexture.filterMode = FilterMode.Point;
    //    settings.terrainMaterial.SetTexture("_RegionTex", regionTexture);
    //    //settings.terrainMaterial.SetFloat("_maxRadius", maxRadius);
    //    //settings.terrainMaterial.SetFloat("_minRadius", minRadius);
    //    regionTexture.Apply();
    //}

    private List<Color> ShuffleColors(List<Color> in_colors)
    {
        System.Random locRando = new System.Random(); // don't like this but temporary
        List<Color> toRet = new List<Color>();
        List<Color> toProcess = in_colors;
        while (toProcess.Count > 0)
        {
            int select = locRando.Next(toProcess.Count);
            toRet.Add(toProcess[select]);
            toProcess.RemoveAt(select);
        }
        return toRet;
    }

    private float CalculateA(float in_pos, float in_lim)
    {
        float inner = Mathf.PI * (in_lim / 3f) * (in_pos / in_lim);
        float toRet = 2f * Mathf.Sin(inner) + 4f;
        return toRet;
    }

    private float CalculateRed(float in_pos, float in_a, float in_b, float in_c)
    {

        float xOffset = in_pos /*- (in_b / 4f)*/;
        float inner = (in_a / in_b) * xOffset;
        float square = Mathf.Pow(inner, 2f);
        float denom = square + 1;
        float left = in_c / denom;
        float right = 1 - in_c;
        float toRet = left + right;

        return toRet;
    }

    private float CalculateRed(float in_pos, float in_lim, float in_a)
    {
        float xOffset = (in_pos / in_lim) + (1f / 3f);
        float left = Mathf.Sin(Mathf.PI * 2f * xOffset) / in_a;
        float toRet = left + (1f - (1f / in_a));
        return toRet;
    }

    private float CalculateGreen(float in_pos, float in_a, float in_b, float in_c)
    {

        float xOffset = in_pos - ((in_b * 2f) / 4f);
        float inner = (in_a / in_b) * xOffset;
        float square = Mathf.Pow(inner, 2f);
        float denom = square + 1;
        float left = in_c / denom;
        float right = 1 - in_c;
        float toRet = left + right;

        return toRet;
    }

    private float CalculateGreen(float in_pos, float in_lim, float in_a)
    {
        float xOffset = (in_pos / in_lim) + (3f / 3f);
        float left = Mathf.Sin(Mathf.PI * 2f * xOffset) / in_a;
        float toRet = left + (1f - (1f / in_a));
        return toRet;
    }

    private float CalculateBlue(float in_pos, float in_a, float in_b, float in_c)
    {

        float xOffset = in_pos - (in_b);
        float inner = (in_a / in_b) * xOffset;
        float square = Mathf.Pow(inner, 2f);
        float denom = square + 1;
        float left = in_c / denom;
        float right = 1 - in_c;
        float toRet = left + right;

        return toRet;
    }

    private float CalculateBlue(float in_pos, float in_lim, float in_a)
    {
        float xOffset = (in_pos / in_lim) + (2f / 3f);
        float left = Mathf.Sin(Mathf.PI * 2f * xOffset) / in_a;
        float toRet = left + (1f - (1f / in_a));
        return toRet;
    }

    public void ToggleRegionalShading(bool ins)
    {
        if (ins)
            regionalShading = 1;
        else
            regionalShading = 0;

        settings.terrainMaterial.SetFloat("_switch", (float)regionalShading);
        settings.OceanMaterial.SetFloat("_switch", (float)regionalShading);
    }
}
