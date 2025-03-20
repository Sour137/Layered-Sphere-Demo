Shader "IE/LandSurfaceShader"
{
    Properties
    {
        //_Color ("Color", Color) = (1,1,1,1)
        _LandTex ("_landTexture", 2D) = "white" {}
        _RegionTex ("_regionTexture", 2D) = "white" {}
        _Switch ("_switch", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        //LOD 200
        
        CGPROGRAM
        #pragma surface surf Lambert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _LandTex;
        sampler2D _RegionTex;
        float _switch;
        //fixed4 _Color;


        struct Input
        {
            float2 uv_LandTex : TEXCOORD0;
            float2 uv3_RegionTex : TEXCOORD2;
            //float float_switch;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float4 a = tex2D (_LandTex, IN.uv_LandTex);
            float4 b = tex2D (_RegionTex, IN.uv3_RegionTex);
            float c = _switch;
            float4 ao = a * (1.0 - c);
            float4 bo = b * c;
            o.Albedo = ao + bo;
            //o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
