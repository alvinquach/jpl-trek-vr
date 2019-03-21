using System.Collections.Generic;
using UnityEngine;
using static TrekVRApplication.XRInteractablePlanetConstants;
using static TrekVRApplication.XRInteractablePlanetUtils;

namespace TrekVRApplication {

    public class XRInteractiblePlanetCoordinateLines : MonoBehaviour {

        private readonly IList<LineRenderer> longitudeLines = new List<LineRenderer>();

        private readonly IList<LineRenderer> latitudeLines = new List<LineRenderer>();

        private Material _coordinateIndicatorMaterial;

        private void Awake() {

            transform.localScale = (Mars.Radius * GlobalTerrainModel.GlobalModelScale + CoordinateIndicatorRadiusOffset) * Vector3.one;

            // Create material for coordinate indicators
            _coordinateIndicatorMaterial = new Material(Shader.Find("Unlit/Color"));
            _coordinateIndicatorMaterial.SetColor("_Color", new Color32(255, 127, 32, 255));

            // Generate latitude and longitude lines
            float angleIncrement = 90.0f / (HemisphereLongLatCoordinateIndicatorCount + 1);

            for (int i = 0; i <= HemisphereLongLatCoordinateIndicatorCount; i++) {
                float latitude = i * angleIncrement;

                GameObject gameObject = new GameObject($"Lat{GameObjectName.PlanetCoordinateLines}{latitudeLines.Count + 1}");
                gameObject.transform.SetParent(transform, false);
                LineRenderer lineRenderer = InitCoordinateIndicator(gameObject, _coordinateIndicatorMaterial);
                GeneratePointsForLatitudeIndicator(lineRenderer, latitude);
                latitudeLines.Add(lineRenderer);

                // We don't need the opposite latitude line for i = 0.
                if (i == 0) {
                    continue;
                }

                gameObject = new GameObject($"Lat{GameObjectName.PlanetCoordinateLines}{latitudeLines.Count + 1}");
                gameObject.transform.SetParent(transform, false);
                lineRenderer = InitCoordinateIndicator(gameObject, _coordinateIndicatorMaterial);
                GeneratePointsForLatitudeIndicator(lineRenderer, -latitude);
                latitudeLines.Add(lineRenderer);
            }

            for (int i = 0; i < 2 * (HemisphereLongLatCoordinateIndicatorCount + 1); i++) {
                float longitude = i * angleIncrement;
                GameObject gameObject = new GameObject($"Lon{GameObjectName.PlanetCoordinateLines}{i + 1}");
                gameObject.transform.SetParent(transform, false);
                LineRenderer lineRenderer = InitCoordinateIndicator(gameObject, _coordinateIndicatorMaterial);
                GeneratePointsForLongitudeIndicator(lineRenderer, longitude);
                longitudeLines.Add(lineRenderer);
            }

        }

    }

}
