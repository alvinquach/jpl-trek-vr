namespace TrekVRApplication {

    public abstract class XRInteractableTerrain : XRInteractableObject {

        protected abstract TerrainBoundingBoxSelectionController BBoxSelectionController { get; }

        public XRInteractableTerrainActivity CurrentActivity { get; protected set; } = XRInteractableTerrainActivity.Default;

        public abstract TerrainModel TerrainModel { get; }

        protected virtual void Awake() {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            terrainModelManager.OnEnableTerrainInteractionChange += EnableTerrainInteraction;
            terrainModelManager.OnEnableTerrainTexturesChange += EnableTerrainTextures;
            terrainModelManager.OnHeightExagerrationChange += SetHeightExagerration;
        }

        protected virtual void Start() {
            BBoxSelectionController.ResetSelectionBoundingBox();
        }

        protected virtual void OnDestroy() {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            terrainModelManager.OnEnableTerrainInteractionChange -= EnableTerrainInteraction;
            terrainModelManager.OnEnableTerrainTexturesChange -= EnableTerrainTextures;
            terrainModelManager.OnHeightExagerrationChange -= SetHeightExagerration;
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
                TerrainModel.LayerController.UseDisabledMaterial = !enabled;
            }
            if (!enabled) {
                SwitchToActivity(XRInteractableTerrainActivity.Disabled);
            } else {
                // TODO ...
            }
        }

        protected virtual void EnableTerrainTextures(bool enabled) {
            if (TerrainModel) {
                TerrainModel.LayerController.DisableTextures = !enabled;
            }
        }

        private void SetHeightExagerration(float scale) {
            TerrainModel.HeightScale = scale;
        }

        public abstract void SwitchToActivity(XRInteractableTerrainActivity activity);

    }

}
