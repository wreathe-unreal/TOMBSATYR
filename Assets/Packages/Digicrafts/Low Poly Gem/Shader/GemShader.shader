// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Digicrafts/GemCrystal"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ReflectionStrength ("Reflection Strength", Range(0.0,1.0)) = 0.5
		_RefractionStrength ("Refraction Strength", Range(0.0,1.0)) = 0.5
		_EnvironmentLight ("Environment Light", Range(0.0,2.0)) = 1.0
		_Emission ("Emission", Range(0.0,2.0)) = 0.0
		_Opacity ("Opacity", Range(0.0,1.0)) = 1.0
		[NoScaleOffset] _RefractTex ("Refraction Texture", Cube) = "" {}
	}
	SubShader {
		Tags {
			"Queue" = "Transparent" 
//			"RenderType"="Transparent"
		}

		Pass {

			Cull Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
        
			struct v2f {
				float4 pos : SV_POSITION;
				float3 uv : TEXCOORD0;
			};

			v2f vert (float4 v : POSITION, float3 n : NORMAL)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v);

				// TexGen CubeReflect:
				// reflect view direction along the normal, in view space.
				float3 viewDir = normalize(ObjSpaceViewDir(v));
				o.uv = -reflect(viewDir, n);
				o.uv = mul(unity_ObjectToWorld, float4(o.uv,0.0f));
				return o;
			}

			fixed4 _Color;
			samplerCUBE _RefractTex;
			half _RefractionStrength;
			half _ReflectionStrength;
			half _EnvironmentLight;
			half _Emission;
			half _Opacity;
			half4 frag (v2f i) : SV_Target
			{
//				half3 refraction = (texCUBE(_RefractTex, i.uv).rgb* _RefractionStrength + 1)/2 * _Color.rgb;
				half3 refraction = texCUBE(_RefractTex, i.uv).rgb*_RefractionStrength*_Color.rgb;
//				half3 refraction = texCUBE(_RefractTex, i.uv).rgb*_Color.rgb;
				half4 reflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.uv);
				reflection.rgb = DecodeHDR (reflection, unity_SpecCube0_HDR) * _ReflectionStrength;
				half3 multiplier = reflection.rgb * _EnvironmentLight + _Emission;
				return half4( refraction.rgb * multiplier.rgb,  _Opacity);
			}
			ENDCG 
		}

		Pass {
			ZWrite On
			Blend One One
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
        
			struct v2f {
				float4 pos : SV_POSITION;
				float3 uv : TEXCOORD0;
				half fresnel : TEXCOORD1;
			};

			v2f vert (float4 v : POSITION, float3 n : NORMAL)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v);

				// TexGen CubeReflect:
				// reflect view direction along the normal, in view space.
				float3 viewDir = normalize(ObjSpaceViewDir(v));
				o.uv = -reflect(viewDir, n);
				o.uv = mul(unity_ObjectToWorld, float4(o.uv,0));
				o.fresnel = 1.0 - saturate(dot(n,viewDir));
				return o;
			}

			fixed4 _Color;
			samplerCUBE _RefractTex;
			half _ReflectionStrength;
			half _RefractionStrength;
			half _EnvironmentLight;
			half _Emission;
			half _Opacity;
			half4 frag (v2f i) : SV_Target
			{
				half3 refraction = texCUBE(_RefractTex, i.uv).rgb * _Color.rgb;
				half4 reflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.uv);
				reflection.rgb = DecodeHDR (reflection, unity_SpecCube0_HDR);
				half3 reflection2 = reflection * _ReflectionStrength * i.fresnel;
				half3 multiplier = reflection.rgb * _EnvironmentLight + _Emission;
				half3 o = reflection2 + refraction.rgb * multiplier;
//				half3 o = refraction.rgb * multiplier;
				half l = o.r * 0.3 + o.g * 0.59 + o.b * 0.11; // Get the brightness 
				return fixed4(_Color.rgb/3 * multiplier + o * _RefractionStrength, l);
//				return fixed4(reflection2 + refraction.rgb * multiplier, _Opacity*l);
			}
			ENDCG
		}

		// lightmap
		Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }
 
            Cull Off
 
            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta
 
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
 
            #include "UnityStandardMeta.cginc"
            ENDCG
        }
        	
        // shadow
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }

	}
}
