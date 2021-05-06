Shader "RG/Effect/UVAlphaAddMask"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
		_AlphaTex("Alpha Texture", 2D) = "white" {}
		_UV_Speed("UV Speed", Vector) = (0, 0, 0, 0)
		_Color_Scale("Colot Scale", Float) = 1
		_MaskScale("Mask Scale",Float) = 1
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off 

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			// #pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv_mask : TEXCOORD1;
				float2 uv_alpha : TEXCOORD2;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			sampler2D _AlphaTex;
			float4 _AlphaTex_ST;
			fixed4 _UV_Speed;
			fixed _Color_Scale;
			float _MaskScale;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_mask = TRANSFORM_TEX(v.uv, _MaskTex);
				o.uv_alpha = TRANSFORM_TEX(v.uv, _AlphaTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				o.uv -= _Time.y * _UV_Speed.xy;
				o.uv_alpha -= _Time.y * _UV_Speed.zw;
				o.uv_mask = (o.uv_mask - 0.5) * _MaskScale + 0.5;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				//clip(tex2D(_MaskTex, i.uv_mask).r - 0.001);

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				col *= tex2D(_AlphaTex, i.uv_alpha).r * _Color_Scale;
				col.a *= tex2D(_MaskTex, i.uv_mask).r;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				
				return col;
			}
			ENDCG
		}
	}
}
