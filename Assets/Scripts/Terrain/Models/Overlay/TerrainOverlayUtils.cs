using UnityEngine;
using UnityEngine.Rendering;

namespace TrekVRApplication {

    public static class TerrainOverlayUtils {

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
