// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DiagonalColor" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 0, 0, 1)
        _Thickness("Thickness", Range(0, 1)) = 0.5
        _Threshold("Threshold", Range(0, 1)) = 0
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
            float _Threshold;
            float _Thickness;
 
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
 
            fixed4 _Color;
            float4 _MainTex_TexelSize;
 
            fixed4 frag(v2f i) : COLOR{
                half4 c = tex2D(_MainTex, i.uv);
                if(i.uv.y < i.uv.x - _Threshold*2 + 1) return c;
                fixed4 newColor = lerp(c, _Color, _Thickness);
                newColor.a = c.a;
                return newColor;
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}