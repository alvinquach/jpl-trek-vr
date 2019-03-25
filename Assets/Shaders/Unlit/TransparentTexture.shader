Shader "Custom/Unlit/TransparentTexture" {

    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0, 1)) = 1.0
    }

    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _Opacity;

            v2f vert (appdata_t v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : COLOR {
                if (_Opacity == 0) {
                    return fixed4(0, 0, 0, 0);
                }
                fixed4 col = tex2D(_MainTex, i.texcoord);
                col.a *= _Opacity;
                return col;
            }

            ENDCG
        }
    }
}
