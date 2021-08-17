Shader "Unlit/Triangle"
{
    Properties
    {
        _Color("Color", Color)=( 1, 1, 1, 1)
        _ThirdSide("Third Side", Range(0, 1)) = 0.25
        _Slope ("Slope", Range(0, 2)) = 1
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

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _ThirdSide;
            float _Slope;

            v2f vert (a2v v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float x = i.uv.x;
                float y = i.uv.y;
                float alpha = saturate(step(_ThirdSide, y) - step(-_Slope * x + _Slope, y) - step(_Slope * x, y));
                return fixed4(_Color.xyz, alpha);
            }
            ENDCG
        }
    }
}
