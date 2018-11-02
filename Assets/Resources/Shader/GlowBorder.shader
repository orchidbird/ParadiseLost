// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Sprite Outline" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
		_BorderSize ("BorderSize", float) = 1
		_Width ("Width", Range(0, 1)) = 0
		_Density ("_Density", Range(0, 1)) = 0
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off
        Blend One OneMinusSrcAlpha
       
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
 
            fixed4 _Color;
            float4 _MainTex_TexelSize;
			float _BorderSize;
 
            fixed4 frag(v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);
                float minX = min(i.uv.x, 1-i.uv.x);
                float minY = min(i.uv.y, 1-i.uv.y);
                float minDist = min(minX, minY);
                if(minDist > _Width) return c;
                return lerp(c, _Color, (1 - minDist/_Width)*_Density); 
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}