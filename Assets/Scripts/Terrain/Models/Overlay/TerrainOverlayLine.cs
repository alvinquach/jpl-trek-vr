using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrekVRApplication {

    [RequireComponent(typeof(LineRenderer))]
    public class TerrainOverlayLine : TerrainOverlayObject {

        private LineRenderer _lineRenderer;

        public override Material Material {
            get => _lineRenderer.material;
            set => _lineRenderer.material = value;
        }

        private float _baseThickness;
        public float BaseThickness {
            get => _baseThickness;
            set {
                if (_baseThickness != value) {
                    _baseThickness = value;
                    UpdateLineThickness();
                }
            }
        }

        protected override void Awake() {
            base.Awake();

            _lineRenderer = GetComponent<LineRenderer>();
            InitLineRenderer();
        }

        public void UpdateLine(IBoundingBox bbox, Vector2 latLonStart, Vector2 latLonEnd) {
            UpdateLine(
                BoundingBoxUtils.CoordinatesToUV(bbox, latLonStart),
                BoundingBoxUtils.CoordinatesToUV(bbox, latLonEnd)
            );
        }

        public void UpdateLine(Vector2 uvStart, Vector2 uvEnd) {
            float horizontalScale = Controller.RenderTextureAspectRatio;

            _lineRenderer.positionCount = 2;

            _lineRenderer.SetPosition(0, new Vector2(horizontalScale * uvStart.x, uvStart.y));
            _lineRenderer.SetPosition(1, new Vector2(horizontalScale * uvEnd.x, uvEnd.y));

            if (gameObject.activeInHierarchy) {
                Controller.UpdateTexture();
            }
        }

        private void UpdateLineThickness() {
            _lineRenderer.startWidth = _baseThickness;
        }

        private void InitLineRenderer() {
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
            _lineRenderer.startWidth = _baseThickness;
            _lineRenderer.loop = false;
            _lineRenderer.material = Material;
        }

    }

}
