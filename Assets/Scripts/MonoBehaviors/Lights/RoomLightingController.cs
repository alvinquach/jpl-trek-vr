using UnityEngine;

public class RoomLightingController : MonoBehaviour {

    private const float DirectionalLightMaxIntensity = 0.8f;

    private const float PointLightMaxIntensity = 0.69f;

    private const float DimIncrement = 0.2f;

    private float _dimAmount = 1;

    public void Dim() {
        _dimAmount -= DimIncrement;
        if (_dimAmount < 0) {
            _dimAmount = 0;
        }
        AdjustLighting(_dimAmount);
    }

    public void Brighten() {
        _dimAmount += DimIncrement;
        if (_dimAmount > 1) {
            _dimAmount = 1;
        }
        AdjustLighting(_dimAmount);
    }

    private void AdjustLighting(float dimAmount) {
        Light[] lights = transform.GetComponentsInChildren<Light>();
        foreach (Light light in lights) {
            if (light.type == LightType.Directional) {
                light.intensity = dimAmount * DirectionalLightMaxIntensity;
            }
            else if (light.type == LightType.Point) {
                light.intensity = dimAmount * PointLightMaxIntensity;
            }
        }
    }

}