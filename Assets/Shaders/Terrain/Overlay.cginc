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