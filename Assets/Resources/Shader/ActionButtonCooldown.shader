Shader "Custom/ActionButtonCooldown" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cooldown("Cooldown", int) = 0
		_RemainTime("RemainTime", int) = 0
		_Alpha("Alpha", float) = 0.5
	}
	SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200
		Cull off

		CGPROGRAM

		#pragma surface surf Lambert alpha:blend

		sampler2D _MainTex;

		float atanXY(float y, float x);
		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		int _Cooldown;
		int _RemainTime;
		float _Alpha;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {
			if (_RemainTime != 0 && _Cooldown != 0) {
				float2 localPos = (IN.uv_MainTex - float2(0.5, 0.5)) * 2;
				float PI = 3.14159265;
				float portion = (float)_RemainTime / _Cooldown;
				float angle = PI / 2 - atanXY(localPos.y, localPos.x);	// 양의 y축으로부터 시계방향으로 잰 각(시계와 같음)
				if (angle < 0)
					angle += PI * 2;
				o.Alpha = _Alpha;
				if ((1 - portion) * 2 * 3.14159265 > angle)
					o.Alpha *= 0.5f;
			}
			else o.Alpha = 0;
		}

		float atanXY(float y, float x) {
			float PI = 3.14159265;
			if (x == 0) {
				if (y > 0) return PI / 2;
				else return -PI / 2;
			}
			float atan1 = atan(y / x);
			if (x > 0) {
				if (y > 0) return atan1;
				else return atan1 + 2 * PI;
			}
			else return atan1 + PI;
		}
		ENDCG
	}
	FallBack "Transparent/VertexLit"
}
