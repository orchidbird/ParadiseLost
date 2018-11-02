// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Sprite Outline" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
		_BorderSize ("BorderSize", float) = 1
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
       
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
 
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
			float _BorderSize;
 
            fixed4 frag(v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);
                // if(c.a != 0) return c;

                _MainTex_TexelSize *= _BorderSize;

                fixed alpha_up = tex2D(_MainTex, i.uv + fixed2(0, _MainTex_TexelSize.y)).a;
                fixed alpha_down = tex2D(_MainTex, i.uv - fixed2(0, _MainTex_TexelSize.y)).a;
                fixed alpha_right = tex2D(_MainTex, i.uv + fixed2(_MainTex_TexelSize.x, 0)).a;
                fixed alpha_left = tex2D(_MainTex, i.uv - fixed2(_MainTex_TexelSize.x, 0)).a;

                if(alpha_down + alpha_left + alpha_right + alpha_up > 0.1) return _Color;
                else return fixed4(0, 0, 0, 0);
            }
 
            ENDCG
        }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
 
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
			float _BorderSize;
 
            fixed4 frag(v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);
                c.rgb *= c.a;
 
                return c;
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}