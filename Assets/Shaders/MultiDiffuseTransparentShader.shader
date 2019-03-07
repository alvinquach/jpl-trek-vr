Shader "Custom/MultiDiffuseTransparentShader"
{
    Properties {
        _DiffuseBase ("Diffuse Base", 2D) = "black" {}
        _Diffuse1 ("Additional Diffuse 1", 2D) = "black" {}
        _Diffuse1Opacity ("Additional Diffuse 1 Opacity", Range(0,1)) = 0.0
        _Diffuse2 ("Additional Diffuse 2", 2D) = "black" {}
        _Diffuse2Opacity ("Additional Diffuse 2 Opacity", Range(0,1)) = 0.0
        _Diffuse3 ("Additional Diffuse 3", 2D) = "black" {}
        _Diffuse3Opacity ("Additional Diffuse 3 Opacity", Range(0,1)) = 0.0
        _Opacity ("Opacity", Range(0,1)) = 1.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		static const int MAX_ADDITIONAL_DIFFUSE_LAYERS = 3;

        sampler2D _DiffuseBase;
        sampler2D _DiffuseAdditional;
        sampler2D _Diffuse1;
        sampler2D _Diffuse2;
        sampler2D _Diffuse3;

		// TODO Add UVs from other textures.
        struct Input {
            float2 uv_DiffuseBase;
        };

		half _Diffuse1Opacity;
		half _Diffuse2Opacity;
		half _Diffuse3Opacity;

		half _Opacity;
        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		half getOpacity(int index) {
			switch (index) {
				case 0:
					return 1.0;
				case 1:
					return _Diffuse1Opacity;
				case 2:
					return _Diffuse2Opacity;
				case 3:
					return _Diffuse3Opacity;
				default:
					return 0.0;
			}
		}

		int getLastOpaqueLayer() {
			for (int i = 3; i > 0; i--) {
				if (getOpacity(i) == 1.0) {
					return i;
				}
			}
			return 0;
		}

		fixed4 calculateAlbedo(float2 uv) {
			fixed4 c = fixed4(0, 0, 0, 1);

			for (int i = getLastOpaqueLayer(); i <= MAX_ADDITIONAL_DIFFUSE_LAYERS; i++) {
				half opacity = getOpacity(i);
				if (opacity == 0.0) {
					continue;
				}

				// For some reason, calling lerp outisde of the switch case gives a 
				// "Sampler parameter must come from a literal expression" error.
				sampler2D diffuse = _DiffuseBase;
				switch (i) {
					case 0:
						c = lerp(c, tex2D(_DiffuseBase, uv), opacity);
						break;
					case 1:
						c = lerp(c, tex2D(_Diffuse1, uv), opacity);
						break;
					case 2:
						c = lerp(c, tex2D(_Diffuse2, uv), opacity);
						break;
					case 3:
						c = lerp(c, tex2D(_Diffuse3, uv), opacity);
						break;
				}
			}
			return c;
		}

        void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = calculateAlbedo(IN.uv_DiffuseBase);
            o.Albedo = c.rgb;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Opacity;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
