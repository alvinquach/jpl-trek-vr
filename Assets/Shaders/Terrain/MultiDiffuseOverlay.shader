Shader "Custom/Terrain/MultiDiffuseOverlay" {

    Properties {
        _Overlay ("Overlay", 2D) = "black" {}
        _OverlayOpacity ("Overlay Opacity", Range(0, 1)) = 0.0
        _DiffuseBase ("Diffuse Base", 2D) = "black" {}
        _Diffuse1 ("Additional Diffuse 1", 2D) = "black" {}
        _Diffuse1Opacity ("Additional Diffuse 1 Opacity", Range(0, 1)) = 0.0
        _Diffuse2 ("Additional Diffuse 2", 2D) = "black" {}
        _Diffuse2Opacity ("Additional Diffuse 2 Opacity", Range(0, 1)) = 0.0
        _Diffuse3 ("Additional Diffuse 3", 2D) = "black" {}
        _Diffuse3Opacity ("Additional Diffuse 3 Opacity", Range(0, 1)) = 0.0
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }

    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		#include "MultiDiffuseOpaque.cginc"

        ENDCG
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

			#include "Overlay.cginc"

            ENDCG
        }
    }
    FallBack "Diffuse"
}
