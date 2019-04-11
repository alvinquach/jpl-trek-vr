using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class GlobeTerrainOverlayController : TerrainOverlayController {

        public static GlobeTerrainOverlayController Instance { get; private set; }

        public override float RenderTextureAspectRatio => 2.0f;

        public override IBoundingBox CurrentBoundingBox => UnrestrictedBoundingBox.Global;

        public GlobeTerrainOverlayController() {
            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                throw new Exception($"Only one instace of {GetType().Name} is allowed!");
            }

            _renderTextureResolution = 2048;
        }

        protected override void Awake() {
            if (Instance != this) {
                Destroy(this);
            }
            base.Awake();
        }

    }

}
