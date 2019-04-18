#include "MultiDiffuse.cginc"

half _DiffuseOpacity;

void surf(Input IN, inout SurfaceOutputStandard o) {
	fixed4 c = calculateAlbedo(IN);
	o.Albedo = c.rgb;
	o.Metallic = _Metallic;
	o.Smoothness = _Glossiness;
	o.Alpha = c.a * _DiffuseOpacity;
}