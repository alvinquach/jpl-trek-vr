using UnityEngine;

namespace TrekVRApplication {

    public class XRInteractableLocalTerrain : XRInteractableTerrain {

        private LocalTerrainBoundingBoxSelectionController _bboxSelectionController;
        protected override TerrainBoundingBoxSelectionController BBoxSelectionController => _bboxSelectionController;

        private LocalTerrainModel _terrainModel;
        public override TerrainModel TerrainModel => _terrainModel;

        #region Unity lifecycle methods

        protected override void Awake() {
            base.Awake();
        }

        protected override void Start() {
            _terrainModel = GetComponent<LocalTerrainModel>();
            _bboxSelectionController = LocalTerrainOverlayController.Instance.BBoxSelectionController;
            base.Start();
        }

        #endregion

        #region Event handlers

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            if (CurrentActivity == XRInteractableTerrainActivity.BBoxSelection) {
                _bboxSelectionController.MakeBoundarySelection(hit);
            }
        }

        public override void OnCursorOver(XRController sender, RaycastHit hit) {
            if (CurrentActivity == XRInteractableTerrainActivity.BBoxSelection) {
                _bboxSelectionController.UpdateCursorPosition(hit);
            }
        }

        #endregion

        private Vector2 GetCoordinatesFromHit(RaycastHit hit) {
            BoundingBox bbox = _terrainModel.SquareBoundingBox;
            return BoundingBoxUtils.UVToCoordinates(bbox, hit.textureCoord);
        }

        public override void SwitchToActivity(XRInteractableTerrainActivity activity) {
            if (CurrentActivity == activity) {
                return;
            }

            // Switching away from current mode.
            switch (CurrentActivity) {
                case XRInteractableTerrainActivity.BBoxSelection:
                    CancelSelection(true, activity);
                    break;
                case XRInteractableTerrainActivity.Disabled:
                    Collider collider = GetComponent<MeshCollider>();
                    if (collider) {
                        collider.enabled = true;
                    }
                    break;
            }

            // Switch to new mode.
            switch (activity) {
                case XRInteractableTerrainActivity.Default:
                    TerrainModel.LayerController.EnableOverlay = false;
                    break;
                case XRInteractableTerrainActivity.BBoxSelection:
                    TerrainModel.LayerController.EnableOverlay = true;
                    _bboxSelectionController.SetEnabled(true);
                    break;
                case XRInteractableTerrainActivity.Disabled:
                    Collider collider = GetComponent<MeshCollider>();
                    if (collider) {
                        collider.enabled = false;
                    }
                    goto case XRInteractableTerrainActivity.Default;
            }

            Debug.Log($"SWITCHING MODES: {CurrentActivity} --> {activity}");
            CurrentActivity = activity;
        }

    }

}