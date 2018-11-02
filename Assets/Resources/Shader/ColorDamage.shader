// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ColorDamage" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Modifier("Modifier", Range(0.5, 2)) = 1
		_Alpha("Alpha", Range(0, 1)) = 1
        _MaxColor ("MaxColor", Color) = (1, 0, 0, 1)
        _MidColor ("MidColor", Color) = (1, 1, 0, 1)
        _MinColor ("MinColor", Color) = (0.5, 0.5, 0.5, 1)
    }
    SubShader { 
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
       
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            float _Width;
            float _Density;
 
            struct v2f {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
            };
 
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
 
            fixed4 _MaxColor;
            fixed4 _MidColor;
            fixed4 _MinColor;
            float4 _MainTex_TexelSize;
			float _Modifier;
			float _Alpha;
 
            fixed4 frag(v2f i) : COLOR{
                half4 c = tex2D(_MainTex, i.uv);
                if(c.a == 0) return c;
				c.a = _Alpha;
				if (c.r < 0.2) return c;
                float modLog = log2(_Modifier);
                fixed4 newColor = _MidColor;
				if (modLog >= 0)
					newColor = lerp(_MidColor, _MaxColor, modLog);
				else
					newColor = lerp(_MidColor, _MinColor, -modLog);
				newColor = lerp(newColor, _MaxColor, i.uv.y);
				newColor.a = _Alpha;
                return newColor;
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}