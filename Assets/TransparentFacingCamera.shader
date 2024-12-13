Shader "Unlit/TransparentFacingCamera"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _TransparencyThreshold ("Transparency Threshold", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100
        Cull Back
        ZWrite On

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        float _Transparency;
        float _TransparencyThreshold;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            
            float facingRatio = dot(normalize(IN.viewDir), normalize(IN.worldNormal));
            
            if (facingRatio > _TransparencyThreshold)
            {
                o.Alpha = _Transparency;
            }
            else
            {
                o.Alpha = 1;
            }
        }
        ENDCG

        // 不透明なパスを追加
        Pass
        {
            ZWrite On
            ColorMask 0
        }
    }
    FallBack "Diffuse"
}
