// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/LeftToRight" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Value ("Value", Range(0, 1)) = 0
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
            float _Value;
 
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
 
            fixed4 frag(v2f i) : COLOR{
                half4 c = tex2D(_MainTex, i.uv);
                if(i.uv.x < _Value) return c;
                c.a = 0;
                return c;
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}