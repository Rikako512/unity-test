Shader "Unlit/CullFrontShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1) // カラーのプロパティを追加
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            Cull Front // フロントフェイスをカリング
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // 色を出力するためのフィールドを追加
            };

            fixed4 _Color; // カラー変数の宣言

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = _Color; // 入力された色を出力に設定
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color; // 出力色を返す
            }
            ENDCG
        }
    }
}
