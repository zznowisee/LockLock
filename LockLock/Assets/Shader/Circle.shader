Shader "Unlit/Circle"{
    Properties {
        _MinRadius ("Min Radius", Range(0, 0.5)) = 0
        _MaxRadius ("Max Radius", Range(0.5, 1)) = 0.5
        _Color ("Color", color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
	    Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

        pass{
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            #include "UnityCG.cginc"

            struct a2v {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _MaxRadius;
            float _MinRadius;
            float4 _Color;

            v2f vertex (a2v v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 fragment (v2f i) : SV_TARGET {
                float3 color = _Color.xyz;

                float2 offset = (i.uv.xy - 0.5) * 2;
                float dst = sqrt(dot(offset, offset));
                float delta = fwidth(dst);
                float alphaMin = smoothstep( _MinRadius - delta, _MinRadius + delta, dst);
                float alphaMax = smoothstep(_MaxRadius - delta, _MaxRadius + delta, dst);
                float alpha = alphaMin - alphaMax;

                return float4(color, alpha);
            }

            ENDCG
        }
    }
}