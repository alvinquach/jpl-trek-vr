using UnityEngine;

namespace TrekVRApplication {

    public abstract class XRInteractableTerrain : XRInteractableObject {

        protected abstract TerrainBoundingBoxSelectionController BBoxSelectionController { get; }

        public XRInteractableTerrainActivity CurrentActivity { get; protected set; } = XRInteractableTerrainActivity.Default;

        public abstract TerrainModel TerrainModel { get; }

        protected virtual void Awake() {
            TerrainModelManager.Instance.OnInteractionStatusChange += EnableTerrainInteraction;
        }

        protected virtual void Start() {
            BBoxSelectionController.ResetSelectionBoundingBox();
        }

        protected virtual void OnDestroy() {
            TerrainModelManager.Instance.OnInteractionStatusChange -= EnableTerrainInteraction;
        }

        /** For bounding box selection. */
        public void CancelSelection(bool cancelAll = false, XRInteractableTerrainActivity nextMode = XRInteractableTerrainActivity.Default) {
            if (CurrentActivity != XRInteractableTerrainActivity.BBoxSelection) {
                return;
            }
            if (BBoxSelectionController.CancelSelection(cancelAll)) {
                CurrentActivity = nextMode;
                BBoxSelectionController.SetEnabled(false);
            }
        }

        protected virtual void EnableTerrainInteraction(bool enabled) {
            if (TerrainModel) {
                TerrainModel.UseDisabledMaterial = !enabled;
            }
            if (!enabled) {
                SwitchToActivity(XRInteractableTerrainActivity.Disabled);
            } else {
                // TODO ...
            }
        }

        public abstract void SwitchToActivity(XRInteractableTerrainActivity activity);

    }

}
