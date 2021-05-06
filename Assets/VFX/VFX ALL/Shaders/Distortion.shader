// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Effect/Distortion" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
	_BumpAmt ("BumpAmt",Range(-100,100)) = 0
}

Category {
	Tags { "Queue"="Transparent+1" "RenderType"="Transparent" }
	Blend One Zero
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off
	
	
	SubShader {
				
			GrabPass {       
				Name "Base"
			}

			Pass {

			Name "Base"
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			float _BumpAmt;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;

			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uvBump : TEXCOORD0;
				float4 uvgrab : TEXCOORD1;
			};
			
			float4 _MainTex_ST;
			fixed4 _TintColor;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvBump = TRANSFORM_TEX(v.texcoord,_MainTex);

#if UNITY_UV_STARTS_AT_TOP  // Direct3D类似平台scale为-1；OpenGL类似平台为1。
				 float scale = -1.0;
#else
				 float scale = 1.0;
#endif
				 o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
				 o.uvgrab.zw = o.vertex.zw;


				return o;
			}

			
			fixed4 frag (v2f i) : SV_Target
			{

				half2 bump = UnpackNormal(tex2D( _MainTex, i.uvBump )).rg;
				float2 offset = bump * _BumpAmt * _GrabTexture_TexelSize.xy;
				i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
				half4 noiseCol = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				return  noiseCol;
			}
			ENDCG 
		}
	}
}
}
