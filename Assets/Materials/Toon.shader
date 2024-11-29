/*
    References:
        https://roystan.net/articles/toon-shader/
        https://en.wikipedia.org/wiki/Blinn–Phong_reflection_model
*/
Shader "bentoBAUX/Toon Lit"
{
    Properties
    {
        [Header(Colours)][Space(10)]
        _DiffuseColour("Diffuse Colour", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        [Toggle(USETRANSPARENT)] _UseTransparent("Use Transparent", float) = 0
        _AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.5

        [Header(Emission)][Space(10)]
        [Toggle(USEEMISSIVE)] _Emissive("Emissive", float) = 0
        [HDR] _EmissiveColour("Emissive Colour", Color) = (1,1,1,1)
        _EmissiveTexture("Emissive Texture", 2D) = "white"{}

        [Header(Normal)][Space(10)]
        [Normal]_Normal("Normal", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0,20)) = 1

        [Header(Blinn Phong)][Space(10)]
        [Toggle(SPECULAR)] _Specular("Specular Highlight", float) = 1

        _k ("Coefficients (Ambient, Diffuse, Specular)", Vector) = (0.5,0.5,0.8)
        _SpecularExponent("Specular Exponent", Float) = 80

        [Header(Rim)][Space(10)]
        [Toggle(RIM)] _Rim("Rim", float) = 1
        _FresnelPower("Fresnel Power", Range(0.01, 1)) = 0.5
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        Pass
        {
            // This pass is rendered from the light sources' perspectives as it writes depth values into the shadowmap.
            // Alpha pixels here are excluded from the shadowmap and therefore will not cast and receive shadows.

            Name "Clip Alphas"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            ZWrite On
            HLSLPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_shadowcaster

            struct appdata
            {
                half4 vertex: POSITION;
                half3 normal: NORMAL;
                half2 uv : TEXCOORD0;
                half4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv_MainTex : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float _AlphaCutoff;

            v2f vertShadow(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv_MainTex = v.uv;
                return o;
            }

            float4 fragShadow(v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv_MainTex);
                clip(c.a - _AlphaCutoff);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            // Base lighting pass for directional light

            Name "ForwardBase"
            Tags
            {
                "LightMode"="ForwardBase"
            }

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma shader_feature RIM
            #pragma shader_feature SPECULAR
            #pragma shader_feature USETRANSPARENT
            #pragma shader_feature USEEMISSIVE


            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            uniform half4 _DiffuseColour;

            uniform sampler2D _MainTex;
            uniform half4 _MainTex_ST;

            uniform half4 _EmissiveColour;
            uniform sampler2D _EmissiveTexture;
            uniform half4 _EmissiveTexture_ST;

            uniform half _AlphaCutoff;

            uniform half4 _Normal_ST;
            uniform sampler2D _Normal;
            uniform half _NormalStrength;

            uniform half4 _LightColor0;
            uniform half3 _k;

            uniform float _SpecularExponent;
            uniform float _FresnelPower;
            uniform float _RimThreshold;

            struct appdata
            {
                half4 vertex: POSITION;
                half3 normal: NORMAL;
                half2 uv : TEXCOORD0;
                half4 tangent : TANGENT;
            };

            struct v2f
            {
                half4 pos: SV_POSITION;
                half2 uv_MainTex : TEXCOORD0;
                half2 uv_Normal : TEXCOORD1;
                half2 uv_Emissive : TEXCOORD2;
                half3 worldPos: TEXCOORD3;
                half3x3 TBN : TEXCOORD4;
                SHADOW_COORDS(7)
                UNITY_FOG_COORDS(8)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.uv_MainTex = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.uv_Normal = v.uv * _Normal_ST.xy + _Normal_ST.zw;
                o.uv_Emissive = v.uv * _EmissiveTexture_ST.xy + _EmissiveTexture_ST.zw;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);

                half3 bitangent = cross(worldNormal, worldTangent);
                half3 worldBitangent = mul((float3x3)unity_ObjectToWorld, bitangent);

                o.TBN = float3x3(worldTangent, worldBitangent, worldNormal);

                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                half3 emissive = half3(0,0,0);
                #ifdef USEEMISSIVE
                emissive = tex2D(_EmissiveTexture, i.uv_Emissive) * _EmissiveColour.rgb;
                #endif

                half4 c = tex2D(_MainTex, i.uv_MainTex) * _DiffuseColour;
                #ifdef USETRANSPARENT
                clip(c.a - _AlphaCutoff);
                #endif

                half3 normalMap = UnpackNormal(tex2D(_Normal, i.uv_Normal));
                normalMap.xy *= _NormalStrength;

                half3 l = normalize(_WorldSpaceLightPos0.xyz);
                half atten = 1.0;

                half3 n = normalize(mul(transpose(i.TBN), normalMap));
                // Transforming normal map vectors from tangent space to world space. TBN * v_world = v_tangent | TBN-1 * v_tangent = v_world
                half3 v = normalize(_WorldSpaceCameraPos - i.worldPos);
                half3 h = normalize(l + v);

                fixed NdotL = saturate(dot(n, l)) * atten;

                fixed shadow = smoothstep(0.0, 0.02, SHADOW_ATTENUATION(i)); // To get rid of shadow artefacts
                float lightIntensity = NdotL * shadow / 0.01;
                lightIntensity = saturate(lightIntensity);

                // Blinn-Phong
                fixed Ia = _k.x;
                fixed Id = _k.y * lightIntensity;

                #ifdef SPECULAR
                float Is = _k.z * pow(saturate(dot(h, n)), _SpecularExponent * _SpecularExponent);
                Is = smoothstep(0.005, 0.01, Is) * atten;
                #else
                float Is = 0.0;  // Disable specular if checkbox is unchecked
                #endif

                half3 skyboxColor = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, float3(0,1,0)).rgb;

                half3 ambient = Ia * c * (UNITY_LIGHTMODEL_AMBIENT + skyboxColor * 0.2);
                half3 diffuse = Id * c * _LightColor0.rgb * shadow;
                half3 specular = Is * _LightColor0.rgb * shadow;

                // Fresnel Rim-Lighting
                half4 fresnel;
                #ifdef RIM
                fixed rimDot = 1 - dot(v, n);
                float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
                rimIntensity = smoothstep(_FresnelPower - 0.01, _FresnelPower + 0.01, rimIntensity);
                fresnel = rimIntensity * _LightColor0;
                #else
                fresnel = 0;
                #endif

                half3 finalColor = ambient + diffuse + specular + fresnel;

                UNITY_APPLY_FOG(i.fogCoord, finalColor);

                return half4(finalColor + emissive, c.a);
            }
            ENDHLSL

        }

        Pass
        {
            // This pass is run for each additional light in the scene.
            // Only directional and point lights are supported.

            Name "ForwardAdd"
            Tags
            {
                "LightMode" = "ForwardAdd"
            }

            Blend One One

            HLSLPROGRAM
            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma shader_feature RIM
            #pragma shader_feature SPECULAR
            #pragma shader_feature USETRANSPARENT

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            uniform half4 _DiffuseColour;

            uniform sampler2D _MainTex;
            uniform half4 _MainTex_ST;

            uniform half _AlphaCutoff;

            uniform half4 _Normal_ST;
            uniform sampler2D _Normal;
            uniform half _NormalStrength;

            uniform half4 _LightColor0;
            uniform half3 _k;

            uniform float _SpecularExponent;
            uniform float _FresnelPower;
            uniform float _RimThreshold;

            struct appdata
            {
                half4 vertex: POSITION;
                half3 normal: NORMAL;
                half2 uv : TEXCOORD0;
                half4 tangent : TANGENT;
            };

            struct v2f
            {
                half4 pos: SV_POSITION;
                half2 uv_MainTex : TEXCOORD0;
                half2 uv_Normal : TEXCOORD1;
                half3 worldPos: TEXCOORD2;
                half3x3 TBN : TEXCOORD3;
                SHADOW_COORDS(6)
                UNITY_FOG_COORDS(7)
                float3 lightDir : TEXCOORD8;
                LIGHTING_COORDS(9, 10)
            };

            v2f vertAdd(appdata v)
            {
                v2f o;
                o.uv_MainTex = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.uv_Normal = v.uv * _Normal_ST.xy + _Normal_ST.zw;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);

                half3 bitangent = cross(worldNormal, worldTangent);
                half3 worldBitangent = mul((float3x3)unity_ObjectToWorld, bitangent);

                o.TBN = float3x3(worldTangent, worldBitangent, worldNormal);

                o.lightDir = ObjSpaceLightDir(v.vertex).xyz;

                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o);

                return o;
            }

            half4 fragAdd(v2f i) : SV_Target
            {
                half4 c = tex2D(_MainTex, i.uv_MainTex) * _DiffuseColour;

                #ifdef USETRANSPARENT
                clip(c.a - _AlphaCutoff);
                #endif

                half3 normalMap = UnpackNormal(tex2D(_Normal, i.uv_Normal));
                normalMap.xy *= _NormalStrength;

                half3 l;
                half atten;

                if (_WorldSpaceLightPos0.w == 0.0)
                {
                    // Directional light
                    l = normalize(_WorldSpaceLightPos0.xyz);
                    atten = 1.0;
                }
                else
                {
                    // Point light
                    l = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);

                    // Calculate attenuation
                    atten = LIGHT_ATTENUATION(i);
                }

                float3 n = normalize(mul(transpose(i.TBN), normalMap));
                // Transforming normal map vectors from tangent space to world space. TBN * v_world = v_tangent | TBN-1 * v_tangent = v_world
                half3 v = normalize(_WorldSpaceCameraPos - i.worldPos);
                half3 h = normalize(l + v);

                float NdotL = saturate(dot(n, l)) * atten;

                fixed shadow = smoothstep(0.0, 0.02, SHADOW_ATTENUATION(i)); // To get rid of shadow artefacts
                float lightIntensity = smoothstep(0, 0.001, NdotL) * shadow;
                lightIntensity = saturate(lightIntensity);

                // Blinn-Phong
                fixed Id = _k.y * lightIntensity;

                #ifdef SPECULAR
                float Is = _k.z * pow(saturate(dot(h, n)), _SpecularExponent * _SpecularExponent);
                Is = smoothstep(0., 0.01, Is) * atten;
                #else
                float Is = 0.0;  // Disable specular if checkbox is unchecked
                #endif

                half3 diffuse = Id * c * _LightColor0.rgb * shadow;
                half3 specular = Is * _LightColor0.rgb * shadow;

                // Fresnel Rim-Lighting
                half4 fresnel;
                #ifdef RIM
                fixed rimDot = 1 - dot(v, n);
                float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
                rimIntensity = smoothstep(_FresnelPower - 0.01, _FresnelPower + 0.01, rimIntensity);
                fresnel = rimIntensity * _LightColor0;
                #else
                fresnel = 0;
                #endif

                half3 finalColor = diffuse + specular + fresnel;

                UNITY_APPLY_FOG(i.fogCoord, finalColor);

                return half4(finalColor, c.a);
            }
            ENDHLSL

        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }


}