using UnityEngine;

namespace TrekVRApplication {

    public abstract class XRController : MonoBehaviour {

        private bool _padTouched = false;

        [SerializeField]
        public GameObject cursor;

        [SerializeField]
        protected GameObject _cameraRig;

        public GameObject CameraRig { get { return _cameraRig; } }

        public SteamVR_TrackedController Controller { get; private set; }

        private void OnEnable() {
            Controller = GetComponent<SteamVR_TrackedController>();

            Controller.TriggerClicked += TriggerClickedHandler;
            Controller.TriggerUnclicked += TriggerUnclickedHandler;
            Controller.PadClicked += PadClickedHandler;
            Controller.PadUnclicked += PadUnclickedHandler;
            Controller.MenuButtonClicked += MenuButtonClickedHandler;
            Controller.Gripped += GrippedHandler;
            Controller.Ungripped += UngrippedHandler;

            // Used internally by this class.
            Controller.PadTouched += OnPadTouched;
            Controller.PadUntouched += OnPadUntouched;
        }

        private void OnDisable() {
            Controller.TriggerClicked -= TriggerClickedHandler;
            Controller.TriggerUnclicked -= TriggerUnclickedHandler;
            Controller.PadClicked -= PadClickedHandler;
            Controller.PadUnclicked -= PadUnclickedHandler;
            Controller.MenuButtonClicked -= MenuButtonClickedHandler;
            Controller.Gripped -= GrippedHandler;
            Controller.Ungripped -= UngrippedHandler;
            Controller.PadTouched -= OnPadTouched;
            Controller.PadUntouched -= OnPadUntouched;
        }

        // Implementing classes should make a super call to this method if
        // it is overwritten.
        protected virtual void Update() {
            if (_padTouched) {
                ClickedEventArgs e = new ClickedEventArgs() {
                    controllerIndex = Controller.controllerIndex,
                    flags = (uint)Controller.controllerState.ulButtonPressed,
                    padX = Controller.controllerState.rAxis0.x,
                    padY = Controller.controllerState.rAxis0.y
                };
                PadSwipeHandler(Controller, e);
            }
        }

        // Used internally by this class.
        private void OnPadTouched(object sender, ClickedEventArgs e) {
            _padTouched = true;
            PadTouchedHandler(sender, e);
        }

        // Used internally by this class.
        private void OnPadUntouched(object sender, ClickedEventArgs e) {
            _padTouched = false;
            PadUntouchedHandler(sender, e);
        }

        protected virtual void TriggerClickedHandler(object sender, ClickedEventArgs e) { }
        protected virtual void TriggerUnclickedHandler(object sender, ClickedEventArgs e) { }
        protected virtual void PadClickedHandler(object sender, ClickedEventArgs e) { }
        protected virtual void PadUnclickedHandler(object sender, ClickedEventArgs e) { }
        protected virtual void MenuButtonClickedHandler(object sender, ClickedEventArgs e) { }
        protected virtual void GrippedHandler(object sender, ClickedEventArgs e) { }
        protected virtual void UngrippedHandler(object sender, ClickedEventArgs e) { }

        // Pad touch handlers are called from this class instead of being 
        // registered to SteamVR_TrackedController as event handlers.
        protected virtual void PadTouchedHandler(object sender, ClickedEventArgs e) { }
        protected virtual void PadUntouchedHandler(object sender, ClickedEventArgs e) { }
        protected virtual void PadSwipeHandler(object sender, ClickedEventArgs e) { }

    }

}