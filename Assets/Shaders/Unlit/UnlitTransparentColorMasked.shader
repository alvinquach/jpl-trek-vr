Shader "Custom/Unlit/TransparentColorMasked"
{
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Transparency Mask", 2D) = "white" {}
    }

    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Unlit alpha:fade
        #pragma target 2.0

        struct Input {
            float2 uv_MainTex;
        };

        fixed4 _Color;
		sampler2D _MainTex;

        void surf(Input IN, inout SurfaceOutput o) {
			o.Emission = _Color.rgb;
			o.Alpha = tex2D(_MainTex, IN.uv_MainTex).a * _Color.a;
		}

		fixed4 LightingUnlit(SurfaceOutput s, fixed3 lightDir, fixed atten) {
			return fixed4(0, 0, 0, s.Alpha);
		}

		ENDCG
    }
}
