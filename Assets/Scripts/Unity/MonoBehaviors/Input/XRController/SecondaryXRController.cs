using UnityEngine;

namespace TrekVRApplication {

    public class SecondaryXRController : XRController {

        [SerializeField]
        private FlashlightController _flashlight;
        public FlashlightController Flashlight => _flashlight;

        [SerializeField]
        private WorldLightingController _worldLights;
        public WorldLightingController WorldLights => _worldLights;

    }

}