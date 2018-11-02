Shader "Custom/FogOfWar" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		TileAlpha ("TileAlpha", float) = 0.5
	    RightAlpha ("RightAlpha", float) = 1
		UpAlpha("UpAlpha", float) = 1
		LeftAlpha("LeftAlpha", float) = 1
		DownAlpha("DownAlpha", float) = 1
	    RightUpAlpha("RightUpAlpha", float) = 1
		LeftUpAlpha("LeftUpAlpha", float) = 1
		LeftDownAlpha("LeftDownAlpha", float) = 1
		RightDownAlpha("RightDownAlpha", float) = 1
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 200
		Cull off
		
		CGPROGRAM

		#pragma surface surf Lambert alpha:blend

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		sampler2D _MainTex;
		float4 _Color;
		float TileAlpha;
		float RightAlpha;
		float UpAlpha;
		float LeftAlpha;
		float DownAlpha;
		float RightUpAlpha;
		float LeftUpAlpha;
		float LeftDownAlpha;
		float RightDownAlpha;
		float lerpAndClamp(float x, float y, float value);
		float lerpByX(float x);
		float lerpByY(float y);
		float lerpByBoth(float x, float y);
		void surf (Input IN, inout SurfaceOutput o) {
			float3 localPos = IN.worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
			float2 localPos2 = mul(float2x2(1, -2,
											1,  2), localPos.xy);
			float x = localPos2.x + 1;
			float y = localPos2.y;

			o.Alpha = min(min(lerpByX(x), lerpByY(y)), lerpByBoth(x, y));

			/*아래 코드와 결과가 같다.
			float a1 = lerpAndClamp(RightDownAlpha, TileAlpha, 2.5 * _x);
			float a2 = lerpAndClamp(RightUpAlpha, TileAlpha, 2.5 * _y);
			float a3 = lerpAndClamp(LeftUpAlpha, TileAlpha, 2.5 * x);
			float a4 = lerpAndClamp(LeftDownAlpha, TileAlpha, 2.5 * y);
			float a5 = lerpAndClamp(RightAlpha, TileAlpha, 2.5 * (_x + _y));
			float a6 = lerpAndClamp(UpAlpha, TileAlpha, 2.5 * (x + _y));
			float a7 = lerpAndClamp(LeftAlpha, TileAlpha, 2.5 * (x + y));
			float a8 = lerpAndClamp(DownAlpha, TileAlpha, 2.5 * (_x + y));
			o.Alpha = min(min(min(a1, a2), min(a3, a4)), min(min(a5, a6), min(a7, a8)));*/
		}
		float lerpAndClamp(float x, float y, float value) {
			if (y <= x)
				return y;
			float result = lerp(x, y, value);
			return clamp(result, x, y);
		}
		float lerpByX(float x) {
			float a;
			if (x < 0.4)
				a = lerpAndClamp(LeftUpAlpha, TileAlpha, 2.5 * x);
			else {
				float _x = 1 - x;
				if (_x < 0.4)
					a = lerpAndClamp(RightDownAlpha, TileAlpha, 2.5 * _x);
				else a = TileAlpha;
			}
			return a;
		}
		float lerpByY(float y) {
			float a;
			if (y < 0.4)
				a = lerpAndClamp(LeftDownAlpha, TileAlpha, 2.5 * y);
			else {
				float _y = 1 - y;
				if (_y < 0.4)
					a = lerpAndClamp(RightUpAlpha, TileAlpha, 2.5 * _y);
				else a = TileAlpha;
			}
			return a;
		}
		float lerpByBoth(float x, float y) {
			float a;
			float _x = 1 - x;
			float _y = 1 - y;
			if (x + y < 0.4)
				a = lerpAndClamp(LeftAlpha, TileAlpha, 2.5 * (x + y));
			else if (x + _y < 0.4)
				a = lerpAndClamp(UpAlpha, TileAlpha, 2.5 * (x + _y));
			else if (_x + y < 0.4)
				a = lerpAndClamp(DownAlpha, TileAlpha, 2.5 * (_x + y));
			else if (_x + _y < 0.4)
				a = lerpAndClamp(RightAlpha, TileAlpha, 2.5 * (_x + _y));
			else a = TileAlpha;
			return a;
		}
		ENDCG
	}
	FallBack "Transparent/VertexLit"
}
