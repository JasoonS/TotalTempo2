Shader "Jason/PostEffectShader"
{
	Properties
	{
		_MainTex ("Jason's Texture", 2D) = "white" {}
		_MustWave ("MustWave", int) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform int _MustWave;

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul( UNITY_MATRIX_MVP, v.vertex );
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag (v2f IN) : SV_Target
			{
				fixed4 col;
				if (_MustWave == 1) {
					col = tex2D(_MainTex, IN.uv + float2(0, sin(IN.uv.x*20 + _Time[1]/2 ) /100));
					col = 1 - col;
				} else {
					col = tex2D(_MainTex, IN.uv);
				}

				return col;
			}
			ENDCG
		}
	}
}
