using System;
using UnityEngine;

namespace TrekVRApplication {

    public abstract class XRController : MonoBehaviour {

        private bool _padTouched = false;

        private DoubleKeyPressTimer _triggerDoubleClickTimer;

        private LongKeyPressTimer _menuButtonLongPressTimer;

        public XRControllerLaserPointer LaserPointer { get; protected set; }

        public SteamVR_TrackedController Controller { get; private set; }

        #region External event handlers

        public event Action<object, ClickedEventArgs> OnTriggerClicked = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnTriggerUnclicked = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnTriggerDoubleClicked = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnPadClicked = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnPadUnclicked = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnMenuButtonClicked = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnMenuButtonUnclicked = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnMenuButtonLongPress = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnGripped = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnUngripped = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnPadTouched = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnPadUntouched = (sender, e) => {};
        public event Action<object, ClickedEventArgs> OnPadSwipe = (sender, e) => {};

        #endregion

        protected virtual void Awake() {
            _triggerDoubleClickTimer = new DoubleKeyPressTimer();
            _triggerDoubleClickTimer.OnActionSuccess += TriggerDoubleClickedInternal;

            _menuButtonLongPressTimer = new LongKeyPressTimer();
            _menuButtonLongPressTimer.OnActionSuccess += MenuButtonLongPressInternal;

            LaserPointer = GetComponent<XRControllerLaserPointer>();
        }

        private void OnEnable() {
            Controller = GetComponent<SteamVR_TrackedController>();

            Controller.TriggerClicked += TriggerClickedHandler;
            Controller.TriggerUnclicked += TriggerUnclickedHandler;
            Controller.PadClicked += PadClickedHandler;
            Controller.PadUnclicked += PadUnclickedHandler;
            Controller.MenuButtonClicked += MenuButtonClickedHandler;
            Controller.MenuButtonUnclicked += MenuButtonUnlickedHandler;
            Controller.Gripped += GrippedHandler;
            Controller.Ungripped += UngrippedHandler;

            // Used internally by this class.
            Controller.PadTouched += PadTouchedInternal;
            Controller.PadUntouched += PadUntouchedInternal;
        }

        private void OnDisable() {
            Controller.TriggerClicked -= TriggerClickedHandler;
            Controller.TriggerUnclicked -= TriggerUnclickedHandler;
            Controller.PadClicked -= PadClickedHandler;
            Controller.PadUnclicked -= PadUnclickedHandler;
            Controller.MenuButtonClicked -= MenuButtonClickedHandler;
            Controller.MenuButtonUnclicked -= MenuButtonUnlickedHandler;
            Controller.Gripped -= GrippedHandler;
            Controller.Ungripped -= UngrippedHandler;
            Controller.PadTouched -= PadTouchedInternal;
            Controller.PadUntouched -= PadUntouchedInternal;
        }

        private void OnDestroy() {

            // Does this need to be unsubscribed, since the timer
            // instances are only referenced in this object, so it
            // will be destroyed anyways?
            _triggerDoubleClickTimer.OnActionSuccess -= TriggerDoubleClickedInternal;
            _menuButtonLongPressTimer.OnActionSuccess -= MenuButtonLongPressInternal;
        }

        // Implementing classes should make a super call to this method if
        // it is overwritten.
        protected virtual void Update() {
            if (_padTouched) {
                PadSwipeHandler(Controller, GenerateClickedEventArgs());
            }
        }

        #region Internally used event handlers

        private void PadTouchedInternal(object sender, ClickedEventArgs e) {
            _padTouched = true;
            PadTouchedHandler(sender, e);
        }

        private void PadUntouchedInternal(object sender, ClickedEventArgs e) {
            _padTouched = false;
            PadUntouchedHandler(sender, e);
        }

        private void TriggerDoubleClickedInternal() {
            TriggerDoubleClickedHandler(Controller, GenerateClickedEventArgs());
        }

        private void MenuButtonLongPressInternal() {
            MenuButtonLongPressHandler(Controller, GenerateClickedEventArgs());
        }

        #endregion

        protected virtual void TriggerClickedHandler(object sender, ClickedEventArgs e) {
            _triggerDoubleClickTimer.RegisterKeyDown();
            OnTriggerClicked(sender, e);
        }

        protected virtual void TriggerUnclickedHandler(object sender, ClickedEventArgs e) {
            _triggerDoubleClickTimer.RegisterKeyUp();
            OnTriggerUnclicked(sender, e);
        }

        protected virtual void TriggerDoubleClickedHandler(object sender, ClickedEventArgs e) {
            OnTriggerDoubleClicked(sender, e);
        }

        protected virtual void PadClickedHandler(object sender, ClickedEventArgs e) {
            OnPadClicked(sender, e);
        }

        protected virtual void PadUnclickedHandler(object sender, ClickedEventArgs e) {
            OnPadUnclicked(sender, e);
        }

        protected virtual void MenuButtonClickedHandler(object sender, ClickedEventArgs e) {
            OnMenuButtonClicked(sender, e);
        }

        protected virtual void MenuButtonUnlickedHandler(object sender, ClickedEventArgs e) {
            OnMenuButtonUnclicked(sender, e);
        }

        protected virtual void MenuButtonLongPressHandler(object sender, ClickedEventArgs e) {
            OnMenuButtonLongPress(sender, e);
        }

        protected virtual void GrippedHandler(object sender, ClickedEventArgs e) {
            OnGripped(sender, e);
        }

        protected virtual void UngrippedHandler(object sender, ClickedEventArgs e) {
            OnUngripped(sender, e);
        }

        // Pad touch handlers are called from this class instead of being 
        // registered to SteamVR_TrackedController as event handlers.

        protected virtual void PadTouchedHandler(object sender, ClickedEventArgs e) {
            OnPadTouched(sender, e);
        }

        protected virtual void PadUntouchedHandler(object sender, ClickedEventArgs e) {
            OnPadUntouched(sender, e);
        }

        protected virtual void PadSwipeHandler(object sender, ClickedEventArgs e) {
            OnPadSwipe(sender, e);
        }

        private ClickedEventArgs GenerateClickedEventArgs() {
            return new ClickedEventArgs() {
                controllerIndex = Controller.controllerIndex,
                flags = (uint)Controller.controllerState.ulButtonPressed,
                padX = Controller.controllerState.rAxis0.x,
                padY = Controller.controllerState.rAxis0.y
            };
        }

    }

}