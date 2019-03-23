using UnityEngine;
using UnityEngine.Rendering;
using static TrekVRApplication.XRInteractableGlobeConstants;

namespace TrekVRApplication {

    public static class XRInteractableGlobeUtils {

        /// <param name="latitude">Angle in degress.</param>
        public static void GeneratePointsForLongitudeIndicator(LineRenderer lineRenderer, float longitude = float.NaN, float scale = 1f) {
            lineRenderer.positionCount = CoordinateIndicatorMaxSegmentCount;
            float angleIncrement = 2 * Mathf.PI / CoordinateIndicatorMaxSegmentCount;
            for (int i = 0; i < CoordinateIndicatorMaxSegmentCount; i++) {
                float angle = i * angleIncrement;
                Vector3 basePosition = scale * new Vector3(0, Mathf.Sin(angle), Mathf.Cos(angle));
                if (float.IsNaN(longitude)) {
                    lineRenderer.SetPosition(i, basePosition);
                }
                else {
                    lineRenderer.SetPosition(i, Quaternion.AngleAxis(longitude, Vector3.up) * basePosition);
                }
            }
        }

        /// <param name="latitude">Angle in degress.</param>
        public static void GeneratePointsForLatitudeIndicator(LineRenderer lineRenderer, float latitude = float.NaN, float scale = 1f) {

            Vector2 offsetAndScale = scale * CalculateLatitudeIndicatorOffsetAndScale(float.IsNaN(latitude) ? 0.0f : latitude);

            int segmentCount = float.IsNaN(latitude) ?
                CoordinateIndicatorMaxSegmentCount :
                (int)(CoordinateIndicatorMaxSegmentCount * (1 - Mathf.Abs(latitude / 200)));

            lineRenderer.positionCount = segmentCount;
            float angleIncrement = 2 * Mathf.PI / segmentCount;

            for (int i = 0; i < segmentCount; i++) {
                float angle = i * angleIncrement;
                Vector3 basePosition = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                if (float.IsNaN(latitude)) {
                    lineRenderer.SetPosition(i, basePosition);
                } else {
                    lineRenderer.SetPosition(i, offsetAndScale.x * basePosition + offsetAndScale.y * Vector3.up);
                }
            }

        }

        /// <param name="latitude">Angle in degress.</param>
        public static Vector2 CalculateLatitudeIndicatorOffsetAndScale(float latitude) {
            latitude *= Mathf.Deg2Rad;
            return new Vector2(
                Mathf.Cos(latitude),    // Horizontal scale
                Mathf.Sin(latitude)     // Vertical offset
            );
        }

        public static LineRenderer InitCoordinateIndicator(GameObject gameObject, Material material, float thickness, bool loop = true) {
            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.startWidth = thickness;
            lineRenderer.loop = loop;
            lineRenderer.material = material;
            return lineRenderer;
        }

    }
}
