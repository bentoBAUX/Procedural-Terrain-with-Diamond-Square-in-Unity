Shader "Custom/VertexDots"
{
    Properties
    {
        _DotColor ("Dot Color", Color) = (1, 1, 1, 1)
        _DotSize ("Dot Size", Float) = 5.0
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" }
        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 pos : POSITION;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float pointSize : PSIZE; // Adjusts point size dynamically
            };

            float4 _DotColor;
            float _DotSize;

            // Vertex Shader: Transforms vertex to clip space
            v2g vert(appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // Geometry Shader: Emit points or quads
            [maxvertexcount(1)] // For points
            void geom(point v2g input[1], inout PointStream<g2f> stream)
            {
                g2f output;
                output.pos = input[0].pos;
                output.color = _DotColor;
                output.pointSize = _DotSize; // Adjust point size
                stream.Append(output);
            }

            // Fragment Shader: Outputs dot colour
            fixed4 frag(g2f i) : SV_Target
            {
                return i.color;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
