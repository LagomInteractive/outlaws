// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Effect/Add/DistortionTex" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
	_SecTex("Dissolve Texture", 2D) = "white" {}
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,1)
	_UVAnim("UVAnim", Vector) = (0,0,1,1)
	_Power("Dissolve", Range(0,1.0)) = 0.1
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off 
	
	
	SubShader {

			Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _SecTex;
			float _Dissolve;
			fixed4 _TintColor;
			fixed4 _UVAnim;
			fixed _Power;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed2 uvAnim = _UVAnim.xy *  _Time.g;
				fixed2  uvOffset = tex2D(_SecTex, i.texcoord + uvAnim).xy;
				fixed4 col =i.color * _TintColor * tex2D(_MainTex, i.texcoord + uvOffset * _Power);
				col.rgb *= 2;
				return col;
			}
			ENDCG 
		}
	}
}
}
