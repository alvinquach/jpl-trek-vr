using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class TerrainHeightProfileController : MonoBehaviour {

        private TerrainModel _terrainModel;

        private TerrainOverlayController _overlayController;

        private Material _lineMaterial;

        private Material _newLineMaterial;

        private readonly Stack<TerrainOverlayLine> _overlayLines = new Stack<TerrainOverlayLine>();

        private readonly Stack<Vector2> _points = new Stack<Vector2>();

        private int _framesSinceLastControllerModalUpdate = 0;

        public bool SelectionInputEnabled { get; private set; } = true;

        /// <summary>
        ///     If true, the controller is operating in height profile mode.
        ///     Otherwise, it is operating in distance mode.
        /// </summary>
        public bool HeightProfileMode { get; private set; }

        private void Awake() {
            InitMaterials();
        }

        private void Start() {
            _terrainModel = GetComponent<TerrainModel>();

            XRInteractableTerrain interactableTerrain = GetComponent<XRInteractableTerrain>();
            if (interactableTerrain is XRInteractableGlobeTerrain) {
                _overlayController = GlobeTerrainOverlayController.Instance;
            } else {
                _overlayController = LocalTerrainOverlayController.Instance;
            }

            // Disable script after initializing.
            enabled = false;
        }

        private void Update() {
            _framesSinceLastControllerModalUpdate++;
        }

        public void SetEnabled(bool enabled, bool heightProfileMode = false) {
            if (this.enabled == enabled) {
                return;
            }
            this.enabled = enabled;
            if (enabled) {
                HeightProfileMode = heightProfileMode;
                SelectionInputEnabled = true;
            }
        }

        public void UpdateCursorPosition(RaycastHit hit) {
            if (!SelectionInputEnabled) {
                return;
            }
            Vector2 currentPoint = hit.textureCoord;
            ControllerModalUpdateCurrentPoint(currentPoint);
            if (_points.Count == 0) {
                return;
            }
            Vector2 previousPoint = _points.Peek();
            if (_overlayLines.Count > 0) {
                TerrainOverlayLine currentOverlayLine = _overlayLines.Peek();
                currentOverlayLine.UpdateLine(previousPoint, currentPoint);
                _overlayController.UpdateTexture();
            }
        }

        public void MakeSelection(RaycastHit hit) {
            if (!SelectionInputEnabled) {
                return;
            }
            Vector2 newPoint = hit.textureCoord;
            if (_points.Count > 0 && _overlayLines.Count > 0) {
                Vector2 previousPoint = _points.Peek();
                TerrainOverlayLine currentOverlayLine = _overlayLines.Peek();
                currentOverlayLine.UpdateLine(previousPoint, newPoint);
                currentOverlayLine.Material = _lineMaterial;
            }
            _points.Push(newPoint);
            TerrainOverlayLine overlayLine = _overlayController.AddLine(_newLineMaterial);
            overlayLine.BaseThickness = 5e-3f; // TODO Unhardcode this
            _overlayLines.Push(overlayLine);
            _overlayController.UpdateTexture();
            ControllerModalAddPoint(newPoint);
        }

        public void CompleteSelection() {
            if (!SelectionInputEnabled || _points.Count < 2) {
                return;
            }

            // Remove the "current line".
            _overlayController.RemoveObject(_overlayLines.Pop());

            ControllerModalShowResults(true);
            SelectionInputEnabled = false;
        }

        public void ResumeSelection() {
            if (SelectionInputEnabled) {
                return;
            }
            SelectionInputEnabled = true;
            ControllerModalShowResults(false);
            if (_points.Count == 0) {
                return;
            }

            // Re-add the "current line".
            TerrainOverlayLine overlayLine = _overlayController.AddLine(_newLineMaterial);
            overlayLine.BaseThickness = 5e-3f; // TODO Unhardcode this
            _overlayLines.Push(overlayLine);
            _overlayController.UpdateTexture();
        }

        public bool CancelSelection(bool cancelAll = false) {
            if (!cancelAll && _overlayLines.Count > 0) {
                _points.Pop();
                _overlayController.RemoveObject(_overlayLines.Pop());
                ControllerModalRemoveLastPoint();
                return false;
            }
            ExitSelectionMode();
            return true;
        }

        private void ExitSelectionMode() {
            ClearLines();

            UserInterfaceManager userInterfaceManager = UserInterfaceManager.Instance;
            if (HeightProfileMode) {
                userInterfaceManager.HideControllerModalsWithActivity(ControllerModalActivity.ToolsHeightProfile);
            } else {
                userInterfaceManager.HideControllerModalsWithActivity(ControllerModalActivity.ToolsDistance);
            }
        }

        private void ClearLines() {
            _overlayLines.ToList()
                .ForEach(line => _overlayController.RemoveObject(line));
            _overlayLines.Clear();
            ControllerModalClearPoints();
        }

        private void ControllerModalAddPoint(Vector2 uv) {
            ControllerModal controllerModal = GetControllerModal();
            if (!controllerModal) {
                return;
            }

            Vector2 latLon = BoundingBoxUtils.UVToCoordinates(GetBoundingBoxFromTerrainModel(), uv);

            string js =
                $"let component = {AngularComponentContainerPath}.{GetControllerModalName()};" +
                $"component && component.addPoint({latLon.x}, {latLon.y});";

            controllerModal.Browser.EvalJS(js);
            _framesSinceLastControllerModalUpdate = 0;
        }

        private void ControllerModalUpdateCurrentPoint(Vector2 uv) {
            if (_framesSinceLastControllerModalUpdate < ControllerModalUpdateInterval) {
                return;
            }
            ControllerModal controllerModal = GetControllerModal();
            if (!controllerModal) {
                return;
            }

            Vector2 latLon = BoundingBoxUtils.UVToCoordinates(GetBoundingBoxFromTerrainModel(), uv);

            string js =
                $"let component = {AngularComponentContainerPath}.{GetControllerModalName()};" +
                $"component && component.updateCurrentPoint({latLon.x}, {latLon.y});";

            controllerModal.Browser.EvalJS(js);
            _framesSinceLastControllerModalUpdate = 0;
        }

        private void ControllerModalRemoveLastPoint() {
            ControllerModal controllerModal = GetControllerModal();
            if (!controllerModal) {
                return;
            }

            string js =
                $"let component = {AngularComponentContainerPath}.{GetControllerModalName()};" +
                $"component && component.removeLastPoint();";

            controllerModal.Browser.EvalJS(js);
            _framesSinceLastControllerModalUpdate = 0;
        }

        private void ControllerModalClearPoints() {
            ControllerModal controllerModal = GetControllerModal();
            if (!controllerModal) {
                return;
            }

            string js =
                $"let component = {AngularComponentContainerPath}.{GetControllerModalName()};" +
                $"component && component.clearPoints();";

            controllerModal.Browser.EvalJS(js);
            _framesSinceLastControllerModalUpdate = 0;
        }

        private void ControllerModalShowResults(bool show) {
            ControllerModal controllerModal = GetControllerModal();
            if (!controllerModal) {
                return;
            }

            string js =
                $"let component = {AngularComponentContainerPath}.{GetControllerModalName()};" +
                $"component && component.showResults({show.ToString().ToLower()});";

            controllerModal.Browser.EvalJS(js);
        }

        private string GetControllerModalName() {
            return HeightProfileMode ? ToolsHeightProfileModalName : ToolsDistanceModalName;
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

        private IBoundingBox GetBoundingBoxFromTerrainModel() {
            if (_terrainModel is GlobeTerrainModel) {
                return UnrestrictedBoundingBox.Global;
            }
            return ((LocalTerrainModel)_terrainModel).SquareBoundingBox;
        }

        private ControllerModal GetControllerModal() {
            ControllerModalActivity activity = HeightProfileMode ? 
                ControllerModalActivity.ToolsHeightProfile :
                ControllerModalActivity.ToolsDistance;
            ControllerModal controllerModal = UserInterfaceManager.Instance.GetControllerModalWithActivity(activity);
            return controllerModal;
        }

    }

}
