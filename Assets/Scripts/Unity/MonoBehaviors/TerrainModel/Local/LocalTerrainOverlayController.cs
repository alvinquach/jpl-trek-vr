using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class LocalTerrainOverlayController : TerrainOverlayController {

        public static LocalTerrainOverlayController Instance { get; private set; }

        private BoundingBox _currentBoundingBox;
        public BoundingBox CurrentBoundingBox {
            get => _currentBoundingBox;
            set {
                _currentBoundingBox = value;
                // TODO Clear all lines
            }
        }

        public LocalTerrainOverlayController() {
            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                throw new Exception($"Only one instace of {GetType().Name} is allowed!");
            }
        }

        protected override void Awake() {
            if (Instance != this) {
                Destroy(this);
            }
            base.Awake();
        }

    }

}
