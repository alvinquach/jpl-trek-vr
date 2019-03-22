using System.Collections.Generic;
using UnityEngine;
using static TrekVRApplication.XRInteractablePlanetConstants;
using static TrekVRApplication.XRInteractablePlanetUtils;

namespace TrekVRApplication {

    public class XRInteractiblePlanetCoordinateLines : MonoBehaviour {

        private readonly IList<LineRenderer> _longitudeLines = new List<LineRenderer>();

        private readonly IList<LineRenderer> _latitudeLines = new List<LineRenderer>();

        private readonly IList<GameObject> _latitudeLabels = new List<GameObject>();

        private float _coordinateLineOpacity = 1.0f;

        private Material _coordinateLineMaterial;

        private float _coordinateLabelOpacity = 1.0f;

        private Material _coordinateLabelMaterial;

        #region Unity lifecycle methods

        private void Awake() {
            GenerateCoordinateLinesAndLabels();
        }

        private void Update() {
            Camera eye = UserInterfaceManager.Instance.XRCamera;
            float distance = Vector3.Distance(eye.transform.position, transform.position);
            UpdateCoordinateLinesOpacity(distance);
            UpdateCoordinateLabelsOpacity(distance);
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
                UpdateMaterialAlpha(_coordinateLineMaterial, _coordinateLineOpacity);

                _coordinateLabelOpacity = distance < CoordinateIndicatorStaticLabelFadeOutDistance ? 1 : 0;
                UpdateMaterialAlpha(_coordinateLabelMaterial, _coordinateLabelOpacity);
            }
        }

        private void GenerateCoordinateLinesAndLabels() {

            // Create material for coordinate lines
            _coordinateLineMaterial = new Material(Shader.Find("Custom/UnlitTransparentColor"));
            _coordinateLineMaterial.SetColor("_Color", CoordinateIndicatorStaticColor);

            // Create material for coordinate labels
            _coordinateLabelMaterial = new Material(Shader.Find("Custom/UnlitTransparentColorMasked"));
            _coordinateLabelMaterial.SetColor("_Color", CoordinateIndicatorStaticColor);

            // Scale
            float linesScale = Mars.Radius * GlobalTerrainModel.GlobalModelScale + CoordinateIndicatorRadiusOffset;
            float labelsScale = Mars.Radius * GlobalTerrainModel.GlobalModelScale + CoordinateIndicatorLabelRadiusOffset;

            // Get template for labels
            GameObject labelTemplate = TemplateService.Instance.GetTemplate(GameObjectName.StaticCoordinateTemplate);

            // Generate latitude and longitude lines
            float angleIncrement = 90.0f / (HemisphereLongLatCoordinateIndicatorCount + 1);

            for (int i = 0; i <= HemisphereLongLatCoordinateIndicatorCount; i++) {
                float latitude = i * angleIncrement;

                // Upper hemisphere
                LineRenderer line = GenerateLatitudeCoordinateLine(latitude, linesScale);

                if (i == 0) {

                    // Make the equator a bit thicker than the rest.
                    line.startWidth = CoordinateIndicatorStaticBoldThickness;

                    // We don't need an opposite set of lines for the equator.
                    // We also don't need the zero degree latitude labels.
                    continue;
                }

                GenerateLatitudeCoordinateLabels(labelTemplate, latitude, labelsScale);

                // Lower hemisphere
                GenerateLatitudeCoordinateLine(-latitude, linesScale);
                GenerateLatitudeCoordinateLabels(labelTemplate, -latitude, labelsScale);
            }

            for (int i = 0; i < 2 * (HemisphereLongLatCoordinateIndicatorCount + 1); i++) {
                GenerateLongitudeCoordinateLine(i * angleIncrement, linesScale);
            }

            for (int i = 1; i < 4 * (HemisphereLongLatCoordinateIndicatorCount + 1); i++) {
                GenerateLongitudeCoordinateLabel(labelTemplate, i * angleIncrement - 180, labelsScale);
            }

        }

        private LineRenderer GenerateLatitudeCoordinateLine(float latitude, float scale) {
            GameObject gameObject = new GameObject($"Lat{GameObjectName.PlanetStaticCoordinateLines}{_latitudeLines.Count + 1}");
            gameObject.transform.SetParent(transform, false);
            LineRenderer lineRenderer = InitCoordinateIndicator(
                gameObject,
                _coordinateLineMaterial,
                CoordinateIndicatorStaticThickness
            );
            GeneratePointsForLatitudeIndicator(lineRenderer, latitude, scale);
            _latitudeLines.Add(lineRenderer);
            return lineRenderer;
        }

        private LineRenderer GenerateLongitudeCoordinateLine(float longitude, float scale) {
            GameObject gameObject = new GameObject($"Lon{GameObjectName.PlanetStaticCoordinateLines}{_longitudeLines.Count + 1}");
            gameObject.transform.SetParent(transform, false);
            LineRenderer lineRenderer = InitCoordinateIndicator(
                gameObject,
                _coordinateLineMaterial,
                CoordinateIndicatorStaticThickness
            );
            GeneratePointsForLongitudeIndicator(lineRenderer, longitude, scale);
            _longitudeLines.Add(lineRenderer);
            return lineRenderer;
        }

        private void GenerateLatitudeCoordinateLabels(GameObject template, float latitude, float positionalScale) {
            string formattedLatitude = string.Format("{0:0.#}", latitude) + "°";

            latitude *= Mathf.Deg2Rad;
            Vector3 basePosition = positionalScale * new Vector3(0, Mathf.Sin(latitude), Mathf.Cos(latitude));

            GameObject label = Instantiate(template, transform, false);
            label.name = $"LatLabel{_latitudeLabels.Count + 1}";
            label.transform.position = basePosition;
            label.SetActive(true);
            POILabel text = label.GetComponent<POILabel>();
            text.Text = formattedLatitude;
            text.Material = _coordinateLabelMaterial;
            _latitudeLabels.Add(label);

            label = Instantiate(label, transform, false);
            label.name = $"LatLabel{_latitudeLabels.Count + 1}";
            basePosition.z *= -1;
            label.transform.position = basePosition;
            _latitudeLabels.Add(label);
        }

        private void GenerateLongitudeCoordinateLabel(GameObject template, float longitude, float positionalScale) {
            string formattedLatitude = string.Format("{0:0.#}", longitude) + "°";

            longitude *= Mathf.Deg2Rad;
            Vector3 basePosition = new Vector3(Mathf.Sin(longitude), CoordinateIndicatorLabelRadiusOffset, Mathf.Cos(longitude));

            GameObject label = Instantiate(template, transform, false);
            label.name = $"LatLabel{_latitudeLabels.Count + 1}";
            label.transform.position = positionalScale * basePosition;
            label.SetActive(true);
            POILabel text = label.GetComponent<POILabel>();
            text.Text = formattedLatitude;
            text.Material = _coordinateLabelMaterial;
            _latitudeLabels.Add(label);
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
                UpdateMaterialAlpha(_coordinateLineMaterial, _coordinateLineOpacity);
            }
        }

        private void UpdateCoordinateLabelsOpacity(float eyeDistance) {
            bool changed = false;

            if (eyeDistance < CoordinateIndicatorStaticLabelFadeInDistance) {
                if (_coordinateLabelOpacity < 1f) {
                    _coordinateLabelOpacity += Time.deltaTime / CoordinateIndicatorStaticFadeDuration;
                    changed = true;
                }
            }
            else if (eyeDistance > CoordinateIndicatorStaticLabelFadeOutDistance && _coordinateLabelOpacity > 0f) {
                _coordinateLabelOpacity -= Time.deltaTime / CoordinateIndicatorStaticFadeDuration;
                changed = true;
            }

            if (changed) {
                _coordinateLabelOpacity = MathUtils.Clamp(_coordinateLabelOpacity, 0f, 1f);
                UpdateMaterialAlpha(_coordinateLabelMaterial, _coordinateLabelOpacity);
            }
        }

        private void UpdateMaterialAlpha(Material material, float alpha) {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }

    }

}
