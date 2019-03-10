using UnityEngine;

namespace TrekVRApplication {

    public class SecondaryXRController : XRController {

        [SerializeField]
        private FlashlightController _flashlight;

        [SerializeField]
        private RoomLightingController _roomLights;

        // TODO Move lighting logic to modal.

        //protected override void TriggerClickedHandler(object sender, ClickedEventArgs e) {
        //    _flashlight?.Toggle();
        //    base.TriggerClickedHandler(sender, e);
        //}

        //protected override void PadClickedHandler(object sender, ClickedEventArgs e) {
        //    if (e.padY > 0) {
        //        _roomLights?.Brighten();
        //    }
        //    else {
        //        _roomLights?.Dim();
        //    }
        //    base.PadClickedHandler(sender, e);
        //}

        //protected override void MenuButtonPressedHandler(object sender, ClickedEventArgs e) {
        //    _flashlight?.CycleNextColor();
        //    base.MenuButtonPressedHandler(sender, e);
        //}

    }

}