Shader "Custom/BloodyScreen" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Black", 2D) = "white" {}
		_HP_Percent("_HP_Percent", float) = 1
	}
	SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200
		Cull off ZWrite off

		CGPROGRAM
		#pragma surface surf Lambert alpha:blend finalcolor:mycolor
		#pragma target 3.0

		sampler2D _MainTex;
		float _HP_Percent;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
		};

		void mycolor(Input IN, SurfaceOutput o, inout fixed4 color)
		{
			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			// 화면 중앙에서 0, 귀퉁이에서 1
			float distanceSquare = ((screenUV.x - 0.5) * (screenUV.x - 0.5) + (screenUV.y - 0.5) * (screenUV.y - 0.5)) * 2;
			color.rgb = lerp(float3(0, 0, 0), float3(0.8 - 0.2 * _HP_Percent, 0, 0), distanceSquare);
			// 체력이 0%일 때 0.2 ~ 0.7
			// 체력이 50%일 때 0 ~ 0.35
			color.a = distanceSquare * 0.5 - _HP_Percent * 0.7 + 0.2;
		}
		void surf(Input IN, inout SurfaceOutput o) {
		}
		ENDCG
	}
	FallBack "Transparent/Diffuse"
}
