using System.Collections.Generic;
using UnityEngine;
using static TrekVRApplication.XRInteractablePlanetConstants;
using static TrekVRApplication.XRInteractablePlanetUtils;

namespace TrekVRApplication {

    public class XRInteractiblePlanetCoordinateLines : MonoBehaviour {

        private readonly IList<LineRenderer> longitudeLines = new List<LineRenderer>();

        private readonly IList<LineRenderer> latitudeLines = new List<LineRenderer>();

        private float _coordinateLineOpacity = 1.0f;

        private Material _coordinateIndicatorMaterial;

        private float _coordinateLabelOpacity = 1.0f;

        private Material _coordinateIndicatorLabelMaterial;

        #region Unity lifecycle methods

        private void Awake() {

            transform.localScale = (Mars.Radius * GlobalTerrainModel.GlobalModelScale + CoordinateIndicatorRadiusOffset) * Vector3.one;

            // Create material for coordinate indicators
            _coordinateIndicatorMaterial = new Material(Shader.Find("Custom/UnlitTransparentColor"));
            _coordinateIndicatorMaterial.SetColor("_Color", CoordinateIndicatorStaticColor);

            // Generate latitude and longitude lines
            float angleIncrement = 90.0f / (HemisphereLongLatCoordinateIndicatorCount + 1);

            for (int i = 0; i <= HemisphereLongLatCoordinateIndicatorCount; i++) {
                float latitude = i * angleIncrement;

                GameObject gameObject = new GameObject($"Lat{GameObjectName.PlanetCoordinateLines}{latitudeLines.Count + 1}");
                gameObject.transform.SetParent(transform, false);
                LineRenderer lineRenderer = InitCoordinateIndicator(
                    gameObject,
                    _coordinateIndicatorMaterial,
                    CoordinateIndicatorStaticThickness
                );
                GeneratePointsForLatitudeIndicator(lineRenderer, latitude);
                latitudeLines.Add(lineRenderer);

                // We don't need the opposite latitude line for i = 0.
                if (i == 0) {
                    continue;
                }

                gameObject = new GameObject($"Lat{GameObjectName.PlanetCoordinateLines}{latitudeLines.Count + 1}");
                gameObject.transform.SetParent(transform, false);
                lineRenderer = InitCoordinateIndicator(
                    gameObject,
                    _coordinateIndicatorMaterial,
                    CoordinateIndicatorStaticThickness
                );
                GeneratePointsForLatitudeIndicator(lineRenderer, -latitude);
                latitudeLines.Add(lineRenderer);
            }

            for (int i = 0; i < 2 * (HemisphereLongLatCoordinateIndicatorCount + 1); i++) {
                float longitude = i * angleIncrement;
                GameObject gameObject = new GameObject($"Lon{GameObjectName.PlanetCoordinateLines}{i + 1}");
                gameObject.transform.SetParent(transform, false);
                LineRenderer lineRenderer = InitCoordinateIndicator(
                    gameObject,
                    _coordinateIndicatorMaterial,
                    CoordinateIndicatorStaticThickness
                );
                GeneratePointsForLongitudeIndicator(lineRenderer, longitude);
                longitudeLines.Add(lineRenderer);
            }

        }

        private void Update() {
            Camera eye = UserInterfaceManager.Instance.XRCamera;
            float distance = Vector3.Distance(eye.transform.position, transform.position);
            UpdateCoordinateLinesOpacity(distance);
        }

        #endregion

        public void SetVisible(bool visible) {
            gameObject.SetActive(visible);
            
            // Disable script if not visible so that it doesn't need to update.
            enabled = visible; 

            if (visible) {
                Camera eye = UserInterfaceManager.Instance.XRCamera;
                float distance = Vector3.Distance(eye.transform.position, transform.position);
                _coordinateLineOpacity = distance < CoordinateIndicatorStaticFadeOutDistance ? 1 : 0;
                UpdateMaterialAlpha(_coordinateIndicatorMaterial, _coordinateLineOpacity);
            }
        }

        private void UpdateCoordinateLinesOpacity(float eyeDistance) {
            bool changed = false;

            if (eyeDistance < CoordinateIndicatorStaticFadeInDistance) {
                if (_coordinateLineOpacity < 1f) {
                    _coordinateLineOpacity += Time.deltaTime / CoordinateIndicatorStaticFadeDuration;
                    changed = true;
                }
            }
            else if (eyeDistance > CoordinateIndicatorStaticFadeOutDistance && _coordinateLineOpacity > 0f) {
                _coordinateLineOpacity -= Time.deltaTime / CoordinateIndicatorStaticFadeDuration;
                changed = true;
            }

            if (changed) {
                _coordinateLineOpacity = MathUtils.Clamp(_coordinateLineOpacity, 0f, 1f);
                UpdateMaterialAlpha(_coordinateIndicatorMaterial, _coordinateLineOpacity);
            }
        }

        private void UpdateMaterialAlpha(Material material, float alpha) {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }

    }

}
