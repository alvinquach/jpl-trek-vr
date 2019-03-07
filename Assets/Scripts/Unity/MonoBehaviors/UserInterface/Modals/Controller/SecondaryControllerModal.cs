using UnityEngine;

namespace TrekVRApplication {

    public class SecondaryControllerModal : ControllerModal {

        public override bool IsPrimary {
            get { return false; }
        }

        protected override void Awake() {
            base.Awake();
            if (!_controller) {
                // TODO Throw exception
            }
            _controller.OnMenuButtonPressed += MenuButtonPressedHandler;
            _controller.OnMenuButtonLongPressed += MenuButtonLongPressedHandler;
            _controller.OnPadClicked += PadClickedHandler;
            _controller.OnPadUnclicked += PadUnclickedHandler;
        }

        private void OnDestroy() {
            _controller.OnMenuButtonPressed -= MenuButtonPressedHandler;
            _controller.OnMenuButtonPressed -= MenuButtonLongPressedHandler;
            _controller.OnPadClicked -= PadClickedHandler;
            _controller.OnPadUnclicked -= PadUnclickedHandler;
        }

        #region Controller event handlers

        private void MenuButtonPressedHandler(object sender, ClickedEventArgs e) {
            MainModal mainModal = UserInterfaceManager.Instance.MainModal;
            switch (CurrentActivity) {
                case ControllerModalActivity.Default:
                    // TODO Turn on secondary controller menu.
                    break;
                case ControllerModalActivity.BBoxSelection:
                    XRInteractablePlanet planet = TerrainModelManager.Instance.GetComponentFromCurrentModel<XRInteractablePlanet>();
                    planet.CancelSelection();
                    break;
                case ControllerModalActivity.BookmarkResults:
                case ControllerModalActivity.ProductResults:
                    mainModal.Visible = true;
                    StartActivity(ControllerModalActivity.Default);
                    break;
            }
        }

        private void MenuButtonLongPressedHandler(object sender, ClickedEventArgs e) {
            StartActivity(ControllerModalActivity.Default);
        }

        private void PadClickedHandler(object sender, ClickedEventArgs e) {

        }

        private void PadUnclickedHandler(object sender, ClickedEventArgs e) {

        }

        #endregion

        public override void StartActivity(ControllerModalActivity activity) {
            if (activity == CurrentActivity) {
                return;
            }
            if (!activity.IsAvailableForSecondary()) {
                Debug.LogError($"Activity {activity} is not available for the secondary controller.");
                return;
            }
            base.StartActivity(activity);
        }

    }
}
