using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class GlobeTerrainModelOverlayController : TerrainModelOverlayController {

        public static GlobeTerrainModelOverlayController Instance;

        public GlobeTerrainModelOverlayController() {
            if (!Instance) {
                Instance = this;
            }  else if (Instance != this) {
                Destroy(this);
                throw new Exception($"Only one instace of {GetType().Name} is allowed!");
            }

            _renderTextureResolution = 1024;
            _renderTextureAspectRatio = 2.0f;
        }

    }

}
