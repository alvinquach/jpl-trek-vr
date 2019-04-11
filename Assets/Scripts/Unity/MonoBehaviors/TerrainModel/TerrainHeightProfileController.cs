using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrekVRApplication {

    public class TerrainHeightProfileController : MonoBehaviour {

        private GameObject _lodContainer;

        private TerrainOverlayController _overlayController;

        private Material _lineMaterial;

        private Material _newLineMaterial;

        private readonly Stack<TerrainOverlayLine> _overlayLines = new Stack<TerrainOverlayLine>();

        private readonly IList<Vector2> _points = new List<Vector2>();

        private void Awake() {
            InitMaterials();
        }

        private void Start() {
            //_lodContainer = transform.Find(GameObjectName.LODGroupContainer).gameObject;

            XRInteractableTerrain interactableTerrain = GetComponent<XRInteractableTerrain>();
            if (interactableTerrain is XRInteractableGlobeTerrain) {
                _overlayController = GlobeTerrainOverlayController.Instance;
            } else {
                _overlayController = LocalTerrainOverlayController.Instance;
            }
        }


        public void SetEnabled(bool enabled) {
            this.enabled = enabled;
        }

        public void UpdateCursorPosition(RaycastHit hit) {
            if (_points.Count == 0) {
                return;
            }
            Vector2 previousPoint = _points[_points.Count - 1];
            Vector2 currentPoint = hit.textureCoord;
            if (_overlayLines.Count > 0) {
                TerrainOverlayLine currentOverlayLine = _overlayLines.Peek();
                currentOverlayLine.UpdateLine(previousPoint, currentPoint);
                _overlayController.UpdateTexture();
            }
        }

        public void MakeSelection(RaycastHit hit) {
            Vector2 newPoint = hit.textureCoord;
            if (_points.Count > 0 && _overlayLines.Count > 0) {
                Vector2 previousPoint = _points[_points.Count - 1];
                TerrainOverlayLine currentOverlayLine = _overlayLines.Peek();
                currentOverlayLine.UpdateLine(previousPoint, newPoint);
                currentOverlayLine.Material = _lineMaterial;
            }
            _points.Add(newPoint);
            TerrainOverlayLine overlayLine = _overlayController.AddLine(_newLineMaterial);
            overlayLine.BaseThickness = 5e-3f; // TODO Unhardcode this
            _overlayLines.Push(overlayLine);
            _overlayController.UpdateTexture();
        }

        public bool CancelSelection(bool cancelAll = false) {
            if (!cancelAll && _overlayLines.Count > 0) {
                _overlayController.RemoveObject(_overlayLines.Pop());
                return false;
            }
            ExitSelectionMode();
            return true;
        }

        private void ExitSelectionMode() {
            _overlayLines.ToList()
                .ForEach(line => _overlayController.RemoveObject(line));
            _overlayLines.Clear();

            UserInterfaceManager uiManager = UserInterfaceManager.Instance;
            uiManager.HideControllerModalsWithActivity(ControllerModalActivity.ToolsProfile);
            uiManager.HideControllerModalsWithActivity(ControllerModalActivity.ToolsDistance);
        }

        private void InitMaterials() {
            int colorId = Shader.PropertyToID("_Color");

            // Create material for selected lines
            _lineMaterial = new Material(Shader.Find("Custom/Unlit/TransparentColor"));
            _lineMaterial.SetColor(colorId, new Color(0, 0.91f, 0, 1));

            // Create material for next line
            _newLineMaterial = new Material(Shader.Find("Custom/Unlit/TransparentColor"));
            _newLineMaterial.SetColor(colorId, Color.yellow);
        }

    }

}
