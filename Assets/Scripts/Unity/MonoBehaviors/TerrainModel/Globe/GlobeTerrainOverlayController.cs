using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class GlobeTerrainOverlayController : TerrainOverlayController {

        public static GlobeTerrainOverlayController Instance { get; private set; }

        public GlobeTerrainOverlayController() {
            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                throw new Exception($"Only one instace of {GetType().Name} is allowed!");
            }

            _renderTextureResolution = 1024;
            _renderTextureAspectRatio = 2.0f;
        }

        protected override void Awake() {
            if (Instance != this) {
                Destroy(this);
            }
            base.Awake();
        }

    }

}
