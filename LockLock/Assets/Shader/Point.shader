Shader "Unlit/Point"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Alpha ("Alpha",Range(0.0,1.0)) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
			};

			struct v2f
			{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
			};

			float4 _Color;
			float _Alpha;

			v2f vert (appdata v)
			{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
			}

			float4 frag (v2f i) : SV_Target
			{
					float2 centreOffset = (i.uv.xy - 0.5) * 2;
					float sqrDst = dot(centreOffset, centreOffset);
					float delta = fwidth(sqrt(sqrDst));
					float alpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);
					alpha *= _Alpha;

					return float4(_Color.xyz, alpha);
			}
			ENDCG
		}
	}
}
