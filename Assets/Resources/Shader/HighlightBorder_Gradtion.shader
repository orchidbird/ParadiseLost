// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Sprite Outline" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
		_BorderSize ("BorderSize", float) = 2
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
       
        Pass {
            CGPROGRAM
            #pragma vertex vert alpha
            #pragma fragment frag alpha
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
 
            float4 _Color;
            float4 _MainTex_TexelSize;
			float _BorderSize;
 
            float4 frag(v2f i) : COLOR
            {
                half4 c = tex2D(_MainTex, i.uv);
                if(c.a != 0) return c;

                float alpha = 0;

                int loop = 0;
                float alpha_up = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                float alpha_down = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                float alpha_right = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                float alpha_left = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                if(alpha_down + alpha_left + alpha_right + alpha_up > 0) alpha += 0.2f;

                loop = 1;
                alpha_up = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                alpha_down = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                alpha_right = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                alpha_left = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                if(alpha_down + alpha_left + alpha_right + alpha_up > 0) alpha += 0.2f;

                loop = 2;
                alpha_up = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                alpha_down = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                alpha_right = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                alpha_left = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                if(alpha_down + alpha_left + alpha_right + alpha_up > 0) alpha += 0.2f;

                loop = 3;
                alpha_up = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                alpha_down = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                alpha_right = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                alpha_left = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                if(alpha_down + alpha_left + alpha_right + alpha_up > 0) alpha += 0.2f;

                loop = 4;
                alpha_up = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                alpha_down = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y*loop*_BorderSize)).a;
                alpha_right = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                alpha_left = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x*loop*_BorderSize, 0)).a;
                if(alpha_down + alpha_left + alpha_right + alpha_up > 0) alpha += 0.2f;

                _Color.a = alpha;
                return _Color;
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}