Shader "Unlit/Terrain"
{
    Properties
    {
        _GrassColour("Grass Colour", Color) = (0,0.3,0,1.0)
        _RockColour("Rock Colour", Color) = (0.2,0.2,0.2,1.0)
        _SnowColour("Snow Colour", Color) = (0.9,.9,.9,1.0)
        _Heights("Heights", Vector) = (0.42,1, 0)
        _Threshold("Threshold", Float) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float height : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GrassColour;
            float4 _RockColour;
            float4 _SnowColour;
            float2 _Heights;
            float _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.height = v.vertex.y;
                return o;
            }

            float4 smoothen(float4 a, float4 b, float height, float edge)
            {
                return lerp(a, b, smoothstep(edge - _Threshold, edge + _Threshold, height));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col;

                if(i.height < _Heights.x - _Threshold)
                {
                    col = _GrassColour;
                } else if (i.height < _Heights.x + _Threshold)
                {
                    col = smoothen(_GrassColour, _RockColour, i.height, _Heights.x);
                } else if (i.height < _Heights.y - _Threshold)
                {
                    col = _RockColour;
                } else if (i.height < _Heights.y + _Threshold)
                {
                    col = smoothen(_RockColour, _SnowColour, i.height, _Heights.y);
                } else
                {
                    col = _SnowColour;
                }

                return col;
            }
            ENDCG
        }
    }
}
