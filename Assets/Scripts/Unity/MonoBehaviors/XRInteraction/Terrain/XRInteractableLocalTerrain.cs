using UnityEngine;

namespace TrekVRApplication {

    public class XRInteractableLocalTerrain : XRInteractableTerrain {

        private TerrainBoundingBoxSelectionController _bboxSelectionController;
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
            switch (CurrentActivity) {
                case XRInteractableTerrainActivity.BBoxSelection:
                    _bboxSelectionController.MakeBoundarySelection(hit);
                    break;
                case XRInteractableTerrainActivity.Distance:
                case XRInteractableTerrainActivity.HeightProfile:
                    HeightProfileController.MakeSelection(hit);
                    break;
            }
        }

        public override void OnCursorOver(XRController sender, RaycastHit hit) {
            switch (CurrentActivity) {
                case XRInteractableTerrainActivity.BBoxSelection:
                    _bboxSelectionController.UpdateCursorPosition(hit);
                    break;
                case XRInteractableTerrainActivity.Distance:
                case XRInteractableTerrainActivity.HeightProfile:
                    HeightProfileController.UpdateCursorPosition(hit);
                    break;
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
                case XRInteractableTerrainActivity.Distance:
                case XRInteractableTerrainActivity.HeightProfile:
                case XRInteractableTerrainActivity.SunAngle:
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
                case XRInteractableTerrainActivity.Distance:
                    TerrainModel.LayerController.EnableOverlay = true;
                    HeightProfileController.SetEnabled(true, false);
                    break;
                case XRInteractableTerrainActivity.HeightProfile:
                    TerrainModel.LayerController.EnableOverlay = true;
                    HeightProfileController.SetEnabled(true, true);
                    break;
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