Shader "Unlit/DottedLine"{
    Properties{
        _MaskTex("Mask Tex", 2D) = "white"{}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader{
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
        pass{
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            #include "UnityCG.cginc"

            struct a2v{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f{
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MaskTex;
            float4 _Color;
            
            v2f vertex (a2v v){
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 fragment (v2f i) : SV_TARGET {
                float3 color;
                color = _Color.rgb;
                float alpha = tex2D(_MaskTex, i.uv).rgb;
                return float4(color, alpha);
            }
            ENDCG
        }
    }
}
