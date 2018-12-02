// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Sprite Outline" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
		_BorderSize ("BorderSize", float) = 1
		[Toggle(MonotoneOverriding)] _MonotoneOverriding("Monotone Overriding", Float) = 0
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off
        Blend One OneMinusSrcAlpha
        ZWrite Off
       
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
 
            struct custom_v2f {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
 
            struct data_from_unity
             {
                 float4 vertex   : POSITION;  // The vertex position in model space.          //  Name&type must be the same!
                 float4 texcoord : TEXCOORD0; // The first UV coordinate.                     //  Name&type must be the same!
                 float4 color    : COLOR;     //    The color value of this vertex specifically. //  Name&type must be the same!
             };
             
            custom_v2f vert(data_from_unity v) {
                custom_v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                return o;
            }
 
            fixed4 _Color;
            float4 _MainTex_TexelSize;
			float _BorderSize;
			float _MonotoneOverriding;
 
            fixed4 frag(custom_v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);
                if(c.a > 0){
                    if(_MonotoneOverriding > 0 && (c.r > 0.1 || c.g > 0.1 || c.b > 0.1))
                        return i.color;
                    return c;
                }

                //c.rgb *= c.a;
                half4 outlineC = _Color;
                //outlineC.a *= ceil(c.a);
                //outlineC.rgb *= outlineC.a;
 
				_MainTex_TexelSize *= _BorderSize;

                fixed alpha_up = tex2D(_MainTex, i.uv + fixed2(0, _MainTex_TexelSize.y)).a;
                fixed alpha_down = tex2D(_MainTex, i.uv - fixed2(0, _MainTex_TexelSize.y)).a;
                fixed alpha_right = tex2D(_MainTex, i.uv + fixed2(_MainTex_TexelSize.x, 0)).a;
                fixed alpha_left = tex2D(_MainTex, i.uv - fixed2(_MainTex_TexelSize.x, 0)).a;

                if(alpha_down + alpha_left + alpha_right + alpha_up >= 0.9f) return outlineC;
                else return fixed4(0, 0, 0, 0);
                //return lerp(outlineC, c, ceil(alpha_up * alpha_down * alpha_right * alpha_left));
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}