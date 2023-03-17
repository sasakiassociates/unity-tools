Shader "HaiThere/PixelFinderCal"
{
    Properties
    {
        _Value ("Range", Range(0.0,1.0)) = 0.5
        _StartColor ("StartColor", Color) = (1.0,1.0,1.0,1.0)
        _EndColor ("EndColor", Color) = (1.0,0.0,0.0,1.0)
    }

    SubShader
    {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
        };

        float4 _StartColor;
        float4 _EndColor;
        float _Value;

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
        {
            surface.Albedo = lerp(_StartColor, _EndColor, _Value);
        }
        ENDCG
    }
    
    FallBack "Diffuse"
}