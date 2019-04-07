using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class GlobeTerrainModelOverlayController : TerrainModelOverlayController {

        public static GlobeTerrainModelOverlayController Instance;

        public GlobeTerrainModelOverlayController() {
            if (Instance == null) {
                Instance = this;
            }  else if (Instance != this) {
                throw new Exception("Only one instace of TerrainModelOverlayController is allowed!");
            }

            _renderTextureResolution = 1024;
            _renderTextureAspectRatio = 2.0f;
        }

    }

}
