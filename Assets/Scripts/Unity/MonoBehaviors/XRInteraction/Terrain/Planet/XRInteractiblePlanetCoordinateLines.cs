using UnityEngine;
using UnityEngine.UI;
using static TrekVRApplication.XRInteractablePlanetConstants;
using static TrekVRApplication.XRInteractablePlanetUtils;

namespace TrekVRApplication {

    public class XRInteractiblePlanetCoordinateLines : MonoBehaviour {

        //private readonly IList<LineRenderer> _longitudeLines = new List<LineRenderer>();

        //private readonly IList<LineRenderer> _latitudeLines = new List<LineRenderer>();

        //private readonly IList<GameObject> _latitudeLabels = new List<GameObject>();

        #region Line and label counts (for naming purposes)

        private int _longitudeLineCount = 0;

        private int _latitudeLineCount = 0;

        private int _longitudeLabelCount = 0;

        private int _latitudeLabelCount = 0;

        #endregion

        private float _coordinateLineOpacity = 1.0f;

        private Material _coordinateLineMaterial;

        private float _coordinateLabelOpacity = 1.0f;

        private Material _coordinateLabelMaterial;

        private Material _verticalAxisMaterial;

        #region Unity lifecycle methods

        private void Awake() {
            GenerateCoordinateLinesAndLabels();
        }

        private void Update() {
            Camera eye = UserInterfaceManager.Instance.XRCamera;
            float distance = Vector3.Distance(eye.transform.position, transform.position);
            UpdateCoordinateLinesOpacity(distance);
            UpdateCoordinateLabelsOpacity(distance);
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
                UpdateMaterialAlpha(_coordinateLabelMaterial, _coordinateLineOpacity);

                _coordinateLabelOpacity = distance < CoordinateIndicatorStaticLabelFadeOutDistance ? 1 : 0;
                UpdateMaterialAlpha(_verticalAxisMaterial, _coordinateLabelOpacity);
            }
        }

        private void GenerateCoordinateLinesAndLabels() {

            // Create material for coordinate lines
            _coordinateLineMaterial = new Material(Shader.Find("Custom/UnlitTransparentColor"));
            _coordinateLineMaterial.SetColor("_Color", CoordinateIndicatorStaticColor);

            // Create material for coordinate labels
            _coordinateLabelMaterial = new Material(Shader.Find("Custom/UnlitTransparentColorMasked"));
            _coordinateLabelMaterial.SetColor("_Color", CoordinateIndicatorStaticColor);

            // Create material for vertical axis
            _verticalAxisMaterial = new Material(Shader.Find("Custom/UnlitTransparentColorMasked"));
            _verticalAxisMaterial.SetColor("_Color", CoordinateIndicatorStaticColor);

            // Scale
            float linesScale = Mars.Radius * GlobalTerrainModel.GlobalModelScale + CoordinateIndicatorRadiusOffset;
            float labelsScale = Mars.Radius * GlobalTerrainModel.GlobalModelScale + CoordinateIndicatorLabelRadiusOffset;

            // Get template for labels
            GameObject labelTemplate = TemplateService.Instance.GetTemplate(GameObjectName.StaticCoordinateTemplate);

            float angleIncrement = 90.0f / (HemisphereLongLatCoordinateIndicatorCount + 1);

            // Latitude lines and labels
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

            // Longitude lines
            for (int i = 0; i < 2 * (HemisphereLongLatCoordinateIndicatorCount + 1); i++) {
                GenerateLongitudeCoordinateLine(i * angleIncrement, linesScale);
            }

            // Longitude labels
            for (int i = 1; i < 4 * (HemisphereLongLatCoordinateIndicatorCount + 1); i++) {
                GenerateLongitudeCoordinateLabel(labelTemplate, i * angleIncrement - 180, labelsScale);
            }

            // Vertical axis
            GenerateVerticalAxis(labelsScale, labelTemplate);
        }

        private LineRenderer GenerateLatitudeCoordinateLine(float latitude, float scale) {
            GameObject gameObject = new GameObject($"Lat{GameObjectName.PlanetStaticCoordinateLines}{++_latitudeLineCount}");
            gameObject.transform.SetParent(transform, false);
            LineRenderer lineRenderer = InitCoordinateIndicator(
                gameObject,
                _coordinateLineMaterial,
                CoordinateIndicatorStaticThickness
            );
            GeneratePointsForLatitudeIndicator(lineRenderer, latitude, scale);
            //_latitudeLines.Add(lineRenderer);
            return lineRenderer;
        }

        private LineRenderer GenerateLongitudeCoordinateLine(float longitude, float scale) {
            GameObject gameObject = new GameObject($"Lon{GameObjectName.PlanetStaticCoordinateLines}{++_longitudeLineCount}");
            gameObject.transform.SetParent(transform, false);
            LineRenderer lineRenderer = InitCoordinateIndicator(
                gameObject,
                _coordinateLineMaterial,
                CoordinateIndicatorStaticThickness
            );
            GeneratePointsForLongitudeIndicator(lineRenderer, longitude, scale);
            //_longitudeLines.Add(lineRenderer);
            return lineRenderer;
        }

        private void GenerateLatitudeCoordinateLabels(GameObject template, float latitude, float positionalScale) {
            string formattedLatitude = string.Format("{0:0.#}", latitude) + "°";

            latitude *= Mathf.Deg2Rad;
            Vector3 basePosition = positionalScale * new Vector3(0, Mathf.Sin(latitude), Mathf.Cos(latitude));

            GameObject label = Instantiate(template, transform, false);
            label.name = $"LatLabel{++_latitudeLabelCount}";
            label.transform.position = basePosition;
            label.SetActive(true);
            Text text = label.GetComponentInChildren<Text>();
            text.text = formattedLatitude;
            text.material = _coordinateLabelMaterial;
            //_latitudeLabels.Add(label);

            // Also generate labels on the opposite side of the globe.
            label = Instantiate(label, transform, false);
            label.name = $"LatLabel{++_latitudeLabelCount}";
            basePosition.z *= -1;
            label.transform.position = basePosition;
            //_latitudeLabels.Add(label);
        }

        private void GenerateLongitudeCoordinateLabel(GameObject template, float longitude, float positionalScale) {
            string formattedLatitude = string.Format("{0:0.#}", longitude) + "°";

            longitude *= Mathf.Deg2Rad;
            Vector3 basePosition = new Vector3(Mathf.Sin(longitude), CoordinateIndicatorLabelRadiusOffset, Mathf.Cos(longitude));

            GameObject label = Instantiate(template, transform, false);
            label.name = $"LatLabel{++_longitudeLabelCount}";
            label.transform.position = positionalScale * basePosition;
            label.SetActive(true);
            Text text = label.GetComponentInChildren<Text>();
            text.text = formattedLatitude;
            text.material = _coordinateLabelMaterial;
            //_longitudeLabels.Add(label);
        }

        private void GenerateVerticalAxis(float positionalScale, GameObject labelTemplate) {
            GameObject gameObject = new GameObject("VerticalAxis");
            gameObject.transform.SetParent(transform, false);

            LineRenderer lineRenderer = InitCoordinateIndicator(
                gameObject,
                _coordinateLineMaterial,
                CoordinateIndicatorStaticBoldThickness
            );
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, positionalScale * Vector3.up);
            lineRenderer.SetPosition(1, positionalScale * Vector3.down);

            // North label
            GameObject label = Instantiate(labelTemplate, transform, false);
            label.name = "NorthLabel";
            label.transform.position = 1.05f * positionalScale * Vector3.up;
            label.SetActive(true);
            Text text = label.GetComponentInChildren<Text>();
            text.text = "N";
            text.fontSize = 36;
            text.material = _verticalAxisMaterial;

            // South label
            label = Instantiate(label, transform, false);
            label.name = "SouthLabel";
            label.transform.position = 1.05f * positionalScale * Vector3.down;
            text = label.GetComponentInChildren<Text>();
            text.text = "S";

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

                // The vertical axis fades in and out along with the coordinate lines,
                // so update the vertical axis material here too.
                UpdateMaterialAlpha(_verticalAxisMaterial, _coordinateLineOpacity);
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
