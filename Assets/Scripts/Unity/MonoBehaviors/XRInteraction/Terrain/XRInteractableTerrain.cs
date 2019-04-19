namespace TrekVRApplication {

    public abstract class XRInteractableTerrain : XRInteractableObject {

        protected abstract TerrainBoundingBoxSelectionController BBoxSelectionController { get; }

        protected TerrainHeightProfileController HeightProfileController { get; private set; }

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
            HeightProfileController = gameObject.AddComponent<TerrainHeightProfileController>();
        }

        protected virtual void OnDestroy() {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            terrainModelManager.OnEnableTerrainInteractionChange -= EnableTerrainInteraction;
            terrainModelManager.OnEnableTerrainTexturesChange -= EnableTerrainTextures;
            terrainModelManager.OnHeightExagerrationChange -= SetHeightExagerration;
        }

        /// <summary>
        ///     For bounding box selection and some tools.
        /// </summary>
        /// <returns>True if the entire selection cancelled, false otherwise.</returns>
        public bool CancelSelection(bool cancelAll = false, XRInteractableTerrainActivity nextMode = XRInteractableTerrainActivity.Default) {
            switch (CurrentActivity) {
                case XRInteractableTerrainActivity.BBoxSelection:
                    if (BBoxSelectionController.CancelSelection(cancelAll)) {
                        CurrentActivity = nextMode;
                        BBoxSelectionController.SetEnabled(false);
                        return true;
                    }
                    break;
                case XRInteractableTerrainActivity.Distance:
                case XRInteractableTerrainActivity.HeightProfile:
                    if (!HeightProfileController.SelectionInputEnabled) {
                        cancelAll = true;
                    }
                    if (HeightProfileController.CancelSelection(cancelAll)) {
                        CurrentActivity = nextMode;
                        HeightProfileController.SetEnabled(false);
                        return true;
                    }
                    break;
                case XRInteractableTerrainActivity.SunAngle:
                    break;
            }
            return false;
        }

        public void CompleteSelection() {
            switch (CurrentActivity) {
                case XRInteractableTerrainActivity.Distance:
                case XRInteractableTerrainActivity.HeightProfile:
                    if (!HeightProfileController.SelectionInputEnabled) {
                        HeightProfileController.ResumeSelection();
                    } else {
                        HeightProfileController.CompleteSelection();
                    }
                    break;
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
