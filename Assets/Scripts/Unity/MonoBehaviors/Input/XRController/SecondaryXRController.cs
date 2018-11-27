using UnityEngine;

namespace TrekVRApplication {

    public class SecondaryXRController : XRController {

        [SerializeField]
        private FlashlightController _flashlight;

        [SerializeField]
        private RoomLightingController _roomLights;

        protected override void TriggerClickedHandler(object sender, ClickedEventArgs e) {
            _flashlight?.Toggle();
        }

        protected override void PadClickedHandler(object sender, ClickedEventArgs e) {
            Debug.Log("Pad clicked at (" + e.padX + ", " + e.padY + ")");

            if (e.padY > 0) {
                _roomLights?.Brighten();
            }
            else {
                _roomLights?.Dim();
            }
        }

        protected override void MenuButtonClickedHandler(object sender, ClickedEventArgs e) {
            _flashlight?.CycleNextColor();
        }

    }

}