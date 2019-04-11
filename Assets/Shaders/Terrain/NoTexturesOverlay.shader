Shader "Custom/Terrain/NoTexturesOverlay" {

    Properties {
        _Overlay ("Overlay", 2D) = "black" {}
        _OverlayOpacity ("Overlay Opacity", Range(0, 1)) = 0.0
        _Color ("Diffuse Color", Color) = (0.5, 0.5, 0.5, 1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader  {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = _Color;
            o.Albedo = c.rgb;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG

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

            sampler2D _Overlay;
            float4 _Overlay_ST;
            half _OverlayOpacity;

            v2f vert (appdata_t v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _Overlay);
                return o;
            }

            fixed4 frag (v2f i) : COLOR {
                if (_OverlayOpacity == 0) {
                    return fixed4(0, 0, 0, 0);
                }
                fixed4 col = tex2D(_Overlay, i.texcoord);
                col.a *= _OverlayOpacity;
                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
