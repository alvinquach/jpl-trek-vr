using UnityEngine;

namespace TrekVRApplication.Scenes.MainRoom {

    [DisallowMultipleComponent]
    public class MainRoomLightingController : WorldLightingController {

        private const float DirectionalLightMaxIntensity = 0.5f;

        private const float TerrainDirectionalLightMinIntensity = 0.2f;

        private const float TerrainDirectionalLightMaxIntensity = 0.75f;

        private const float PointLightMaxIntensity = 0.69f;

        private const float SpotLightMaxIntensity = 0.69f;

        private const float SkylightMinRange = 9.5f;

        private const float SkylightMaxRange = 40.0f;

        private const float DimIncrement = 0.2f;

        private float _dimAmount = 1;

        public override void Dim() {
            _dimAmount -= DimIncrement;
            if (_dimAmount < 0) {
                _dimAmount = 0;
            }
            AdjustLighting(_dimAmount);
        }

        public override void Brighten() {
            _dimAmount += DimIncrement;
            if (_dimAmount > 1) {
                _dimAmount = 1;
            }
            AdjustLighting(_dimAmount);
        }

        private void AdjustLighting(float dimAmount) {
            Light[] lights = transform.GetComponentsInChildren<Light>();
            foreach (Light light in lights) {
                switch (light.type) {
                    case LightType.Directional:
                        if (light.cullingMask == 1 << (int)CullingLayer.Terrain) {
                            light.intensity = Mathf.Lerp(
                                TerrainDirectionalLightMinIntensity, 
                                TerrainDirectionalLightMaxIntensity, 
                                dimAmount
                            );
                        }
                        else {
                            light.intensity = dimAmount * DirectionalLightMaxIntensity;
                        }
                        break;
                    case LightType.Point:
                        light.intensity = dimAmount * PointLightMaxIntensity;
                        break;
                    case LightType.Spot:
                        if (light.gameObject.name.Contains("Skylight")) {
                            light.range = Mathf.Lerp(SkylightMinRange, SkylightMaxRange, dimAmount);
                        }
                        else {
                            light.intensity = dimAmount * SpotLightMaxIntensity;
                        }
                        break;
                }
            }
        }

    }

}