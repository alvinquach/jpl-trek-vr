using UnityEngine;
using static TrekVRApplication.TerrainConstants;
using static TrekVRApplication.TerrainOverlayUtils;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public abstract class TerrainBoundingBoxSelectionController : MonoBehaviour {

        protected abstract TerrainOverlayController OverlayController { get; }

        protected abstract float IndicatorThickness { get; }

        protected abstract float IndicatorActiveThickness { get; }

        protected Material _coordinateIndicatorMaterial;

        protected Vector4 _selectionBoundingBox;
        public Vector4 SelectionBoundingBox {
            get => _selectionBoundingBox;
        }

        protected LineRenderer _lonSelectionStartIndicator;
        protected LineRenderer _latSelectionStartIndicator;
        protected LineRenderer _lonSelectionEndIndicator;
        protected LineRenderer _latSelectionEndIndicator;

        protected byte _selectionIndex = 0;

        protected LineRenderer CurrentSelectionIndicator => GetSelectionIndicatorByIndex(_selectionIndex);

        protected int _framesSinceLastControllerModalUpdate = 0;

        #region Unity lifecycle methods

        protected virtual void Awake() {
            GenerateSelectionIndicatorLines();
        }

        protected virtual void OnEnable() {
            ActivateCurrentIndicator();
        }

        #endregion

        public void SetEnabled(bool enabled) {
            gameObject.SetActive(enabled);
        }

        public virtual void MakeBoundarySelection(RaycastHit hit) {

            Vector2 coord = GetCoordFromHit(hit);
            float angle;

            // Longitude selection
            if (_selectionIndex % 2 == 0) {
                angle = coord.y;
                Debug.Log($"Lon selection: {angle} degrees");
            }

            // Latitude selection
            else {
                angle = coord.x;
                Debug.Log($"Lat selection: {angle} degrees");
            }

            _selectionBoundingBox[_selectionIndex] = angle;

            // TODO Calculate line thickness to account for texture projection.
            CurrentSelectionIndicator.startWidth = IndicatorThickness;
            _selectionIndex++;


            // Check if selection is finished.
            if (_selectionIndex == 4) {
                Debug.Log("Selection Complete: " + _selectionBoundingBox);

                // Create localized terrain model from the selection.
                TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
                TerrainModel terrainModel = terrainModelManager.CreateLocalModelFromSubset(
                    terrainModelManager.CurrentVisibleModel,
                    _selectionBoundingBox
                );

                // Make the new localized terrain model visible
                terrainModelManager.ShowTerrainModel(terrainModel, false);
                ExitSelectionMode();
            }
            else {
                ActivateCurrentIndicator();
                OverlayController.UpdateTexture();
            }

            SendBoundingBoxUpdateToControllerModal(new BoundingBox(_selectionBoundingBox));
        }

        /// <returns>The latitude or longitude angle.</returns>
        public abstract float UpdateCursorPosition(RaycastHit hit);

        public bool CancelSelection(bool cancelAll = false) {

            if (!cancelAll && _selectionIndex > 0) {

                // Hide the previous indicator
                CurrentSelectionIndicator.enabled = false;

                // Reset selection value
                _selectionBoundingBox[_selectionIndex--] = float.NaN;

                // Show current indicator
                ActivateCurrentIndicator();

                return false;
            }

            ExitSelectionMode();
            return true;
        }

        public void ResetSelectionBoundingBox() {
            _selectionBoundingBox = new Vector4(float.NaN, float.NaN, float.NaN, float.NaN);
        }

        protected virtual void ExitSelectionMode() {
            ResetSelectionBoundingBox();
            _selectionIndex = 0;
            ResetIndicatorPositions(true);
            UserInterfaceManager.Instance.HideControllerModalsWithActivity(ControllerModalActivity.BBoxSelection);
        }

        protected LineRenderer GetSelectionIndicatorByIndex(int index) {
            switch (index) {
                case 0:
                    return _lonSelectionStartIndicator;
                case 1:
                    return _latSelectionStartIndicator;
                case 2:
                    return _lonSelectionEndIndicator;
                case 3:
                    return _latSelectionEndIndicator;
                default:
                    return null;
            }
        }

        protected void SendBoundingBoxUpdateToControllerModal(BoundingBox bbox) {
            ControllerModal controllerModal = UserInterfaceManager.Instance
                .GetControllerModalWithActivity(ControllerModalActivity.BBoxSelection);

            if (!controllerModal) {
                return;
            }

            string js =
                $"let component = {AngularComponentContainerPath}.{BoundingBoxSelectionModalName};" +
                $"component && component.updateBoundingBox({bbox.ToString(", ", 7)}, {_selectionIndex});";

            controllerModal.Browser.EvalJS(js);
            _framesSinceLastControllerModalUpdate = 0;
        }

        protected void ActivateCurrentIndicator() {
            // TODO Calculate line thickness to account for texture projection.
            CurrentSelectionIndicator.startWidth = IndicatorActiveThickness;
            CurrentSelectionIndicator.enabled = true;
        }

        protected virtual void GenerateSelectionIndicatorLines() {

            // Create material for coordinate indicators
            _coordinateIndicatorMaterial = new Material(Shader.Find("Unlit/Color"));
            _coordinateIndicatorMaterial.SetColor("_Color", CoordinateIndicatorColor);

            _lonSelectionStartIndicator = DrawRenderTextureLine(
                Vector2.zero,
                Vector2.up,
                transform,
                _coordinateIndicatorMaterial,
                IndicatorThickness,
                $"Lon{GameObjectName.SelectionIndicator}1"
            );

            _latSelectionStartIndicator = DrawRenderTextureLine(
                Vector2.zero,
                Vector2.right,
                transform,
                _coordinateIndicatorMaterial,
                IndicatorThickness,
                $"Lat{GameObjectName.SelectionIndicator}1"
            );

            _lonSelectionEndIndicator = DrawRenderTextureLine(
                Vector2.right,
                Vector2.one,
                transform,
                _coordinateIndicatorMaterial,
                IndicatorThickness,
                $"Lon{GameObjectName.SelectionIndicator}2"
            );

            _latSelectionEndIndicator = DrawRenderTextureLine(
                Vector2.up,
                Vector2.one,
                transform,
                _coordinateIndicatorMaterial,
                IndicatorThickness,
                $"Lat{GameObjectName.SelectionIndicator}2"
            );

        }

        protected virtual void ResetIndicatorPositions(bool disable) {
            for (int i = 0; i < 4; i++) {
                LineRenderer lineRenderer = GetSelectionIndicatorByIndex(i);
                if (i % 2 == 0) {
                    lineRenderer.SetPosition(0, new Vector2(0, 0));
                    lineRenderer.SetPosition(1, new Vector2(0, 1));
                }
                else {
                    lineRenderer.SetPosition(0, new Vector2(0, 0));
                    lineRenderer.SetPosition(1, new Vector2(1, 0));
                }
                if (disable) {
                    lineRenderer.enabled = false;
                }
            }
            OverlayController.UpdateTexture();
        }

        protected abstract Vector2 GetCoordFromHit(RaycastHit hit);

        /// <summary>
        ///     Helper method for generating/updated longitude and
        ///     latitude indicators on the globe model.
        /// </summary>
        /// <param name="latitude">Angle in degress.</param>
        private Vector2 CalculateLatitudeIndicatorOffsetAndScale(float latitude) {
            latitude *= Mathf.Deg2Rad;
            return new Vector2(
                Mathf.Cos(latitude),    // Horizontal scale
                Mathf.Sin(latitude)     // Vertical offset
            );
        }

        /// <summary>
        ///     Draws a line inside the render texture camera area for
        ///     display on terrain sections.
        /// </summary>
        private LineRenderer DrawRenderTextureLine(Vector2 start, Vector2 end, Transform parent,
            Material material, float baseThickness, string name = "Line") {

            GameObject gameObject = new GameObject(name) {
                layer = (int)CullingLayer.RenderToTexture
            };
            gameObject.transform.SetParent(parent, false);
            LineRenderer lineRenderer = InitCoordinateIndicator(gameObject, material, baseThickness, false);
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            return lineRenderer;
        }

    }

}
