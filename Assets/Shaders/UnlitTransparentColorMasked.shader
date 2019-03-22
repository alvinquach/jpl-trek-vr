Shader "Custom/UnlitTransparentColorMasked"
{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Transparency Mask", 2D) = "white" {}
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(0)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;

            v2f vert (appdata_t v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : COLOR {
                fixed4 col = _Color;
				col.a *= tex2D(_MainTex, i.texcoord).a;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

			ENDCG
        }
    }
}
