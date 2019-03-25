using UnityEngine;

namespace TrekVRApplication {

    public class SecondaryXRController : XRController {

        [SerializeField]
        private FlashlightController _flashlight;
        public FlashlightController Flashlight {
            get => _flashlight;
        }

        [SerializeField]
        private RoomLightingController _roomLights;
        public RoomLightingController RoomLights {
            get => _roomLights;
        }

    }

}