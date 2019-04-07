using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class LocalTerrainOverlayController : TerrainOverlayController {

        public static LocalTerrainOverlayController Instance { get; private set; }

        public override float RenderTextureAspectRatio => 1.0f;

        private BoundingBox _currentBoundingBox = BoundingBox.Zero;
        public override IBoundingBox CurrentBoundingBox => _currentBoundingBox;

        public LocalTerrainOverlayController() {
            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                throw new Exception($"Only one instace of {GetType().Name} is allowed!");
            }
        }

        #region Unity lifecycle functions

        protected override void Awake() {
            if (Instance != this) {
                Destroy(this);
            }
            base.Awake();
        }

        private void Start() {
            TerrainModelManager.Instance.OnCurrentTerrainModelChange += OnTerrainModelChange;
        }

        private void OnDestroy() {
            TerrainModelManager.Instance.OnCurrentTerrainModelChange -= OnTerrainModelChange;
        }

        #endregion

        private void OnTerrainModelChange(TerrainModel terrainModel) {
            if (terrainModel is LocalTerrainModel) {
                _currentBoundingBox = ((LocalTerrainModel)terrainModel).SquareBoundingBox;
            }
            else {
                _currentBoundingBox = BoundingBox.Zero;
            }
        }

    }

}
