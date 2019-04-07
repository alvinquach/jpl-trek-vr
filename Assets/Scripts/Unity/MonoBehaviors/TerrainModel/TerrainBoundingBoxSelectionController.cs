using UnityEngine;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public abstract class TerrainBoundingBoxSelectionController : MonoBehaviour {

        protected const int ControllerModalBoundingBoxUpdateInterval = 10;

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

        public abstract void MakeBoundarySelection(RaycastHit hit);

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

        protected abstract void ActivateCurrentIndicator();

        protected abstract void GenerateSelectionIndicatorLines();

        protected abstract void ResetIndicatorPositions(bool disable);

    }

}
