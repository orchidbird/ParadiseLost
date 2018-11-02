// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/2D/Dissolve"
{
    Properties {
        [PerRendererData] _MainTex ("Main texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0., 1.01)) = 0.
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
 
    SubShader {
 
        Tags { "Queue"="Transparent" }
        Cull Off
		Lighting Off
		ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
 
        Pass {
           
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
           
            sampler2D _MainTex;
 
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
 
            float _Threshold;
 
            fixed4 frag(v2f i) : SV_Target {
                float4 c = tex2D(_MainTex, i.uv);
                float val = tex2D(_MainTex, i.uv).r;
                float minimum = 0.3f;
                c.r *= (1-minimum) * step(_Threshold*1.25f, val) + minimum;
                c.g *= step(_Threshold*1.4f, val);
                c.b *= step(_Threshold*1.4f, val);
                c.a *= step(_Threshold+0.01f, val);
                return c;
            }
            ENDCG
        }
    }
}