using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class LocalTerrainModelOverlayController : TerrainModelOverlayController {

        public static LocalTerrainModelOverlayController Instance;

        private BoundingBox _currentBoundingBox;
        public BoundingBox CurrentBoundingBox {
            get => _currentBoundingBox;
            set {
                _currentBoundingBox = value;
                // TODO Clear all lines
            }
        }

        public LocalTerrainModelOverlayController() {
            if (!Instance) {
                Instance = this;
            }  else if (Instance != this) {
                Destroy(this);
                throw new Exception($"Only one instace of {GetType().Name} is allowed!");
            }
        }

    }

}
