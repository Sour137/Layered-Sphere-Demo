Shader "IE/OceanSurfaceShader"
{
    Properties
    {
        //_LightColor ("LightColor", Color) = (1,1,1,1)
        _OceanTex ("_OceanTex", 2D) = "white" {}
        _RegionTex ("_regionTexture", 2D) = "white" {}
        _Switch ("_switch", float) = 0
    }
    SubShader
    {
        CGPROGRAM
        #pragma surface surf Lambert 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _OceanTex;
        sampler2D _RegionTex;
        float _switch;


        //fixed4 _LightColor;

        struct Input
        {
            float2 uv_OceanTex;
            float2 uv3_RegionTex : TEXCOORD2;

        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float4 c = tex2D(_OceanTex, IN.uv_OceanTex);
            float4 d = tex2D(_RegionTex, IN.uv3_RegionTex);
            float e = _switch;
            float4 co = c * (1.0 - e);
            float4 dout = d * e;
            o.Albedo = co + dout;
            //o.Alpha = c.a;
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
