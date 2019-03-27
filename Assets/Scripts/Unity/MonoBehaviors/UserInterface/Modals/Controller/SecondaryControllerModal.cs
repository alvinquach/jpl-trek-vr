using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public class SecondaryControllerModal : ControllerModal {

        private UnityBrowserTerrainModelFunctions _terrainModelFunctions;

        private KeyCode _padCurrentKey = 0;

        public override bool IsPrimary {
            get => false;
        }

        protected override void Awake() {
            base.Awake();
            if (!_controller) {
                // TODO Throw exception
            }
            _controller.OnTriggerClicked += TriggerClickedHandler;
            _controller.OnMenuButtonPressed += MenuButtonPressedHandler;
            _controller.OnMenuButtonLongPressed += MenuButtonLongPressedHandler;
            _controller.OnPadClicked += PadClickedHandler;
            _controller.OnPadUnclicked += PadUnclickedHandler;
        }

        private void OnDestroy() {
            _controller.OnTriggerClicked -= TriggerClickedHandler;
            _controller.OnMenuButtonPressed -= MenuButtonPressedHandler;
            _controller.OnMenuButtonPressed -= MenuButtonLongPressedHandler;
            _controller.OnPadClicked -= PadClickedHandler;
            _controller.OnPadUnclicked -= PadUnclickedHandler;
        }

        protected override void Init(Mesh mesh) {
            base.Init(mesh);
            _terrainModelFunctions = new UnityBrowserTerrainModelFunctions(Browser);
        }

        protected override void OnBrowserLoad(JSONNode loadData) {
            _terrainModelFunctions.RegisterFunctions();
        }

        #region Controller event handlers

        private void TriggerClickedHandler(object sender, ClickedEventArgs e) {
            // TODO Only toggle flashlight when default activity is active.
            SecondaryXRController controller = (SecondaryXRController)_controller;
            controller.Flashlight?.Toggle();
        }

        private void MenuButtonPressedHandler(object sender, ClickedEventArgs e) {
            MainModal mainModal = UserInterfaceManager.Instance.MainModal;
            switch (CurrentActivity) {
                case ControllerModalActivity.Default:
                    // TODO Turn on secondary controller menu instead.
                    SecondaryXRController controller = (SecondaryXRController)_controller;
                    controller.Flashlight?.CycleNextColor();
                    break;
                case ControllerModalActivity.BBoxSelection:
                    XRInteractableGlobe globe = TerrainModelManager.Instance.GetComponentFromCurrentModel<XRInteractableGlobe>();
                    globe.CancelSelection();
                    if (globe.CurrentActivity != XRInteractableTerrainActivity.BBoxSelection) {
                        mainModal.Visible = true;
                    }
                    break;
                case ControllerModalActivity.BookmarkResults:
                case ControllerModalActivity.ProductResults:
                case ControllerModalActivity.LayerManager:
                    mainModal.Visible = true;
                    StartActivity(ControllerModalActivity.Default);
                    break;
            }
        }

        private void MenuButtonLongPressedHandler(object sender, ClickedEventArgs e) {
            StartActivity(ControllerModalActivity.Default);
        }

        private void PadClickedHandler(object sender, ClickedEventArgs e) {
            if (CurrentActivity == ControllerModalActivity.Default) {
                SecondaryXRController controller = (SecondaryXRController)_controller;
                if (e.padY > 0) {
                    controller.RoomLights?.Brighten();
                } else {
                    controller.RoomLights?.Dim();
                }
            }
            else {
                if (_padCurrentKey != 0) {
                    return;
                }
                if (Mathf.Abs(e.padX) > Mathf.Abs(e.padY)) {
                    _padCurrentKey = e.padX < 0 ? KeyCode.A : KeyCode.D;
                } else {
                    _padCurrentKey = e.padY < 0 ? KeyCode.S : KeyCode.W;
                }
                Input.RegisterKeyDown(_padCurrentKey);
            }
        }

        private void PadUnclickedHandler(object sender, ClickedEventArgs e) {
            if (_padCurrentKey == 0) {
                return;
            }
            Input.RegisterKeyUp(_padCurrentKey);
            _padCurrentKey = 0;
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
