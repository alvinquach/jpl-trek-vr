using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public class SecondaryControllerModal : ControllerModal {

        private UnityBrowserWebFunctions _webFunctions;
        private UnityBrowserSearchFunctions _searchFunctions;
        private UnityBrowserUserInterfaceFunctions _userInterfaceFunctions;
        private UnityBrowserTerrainModelFunctions _terrainModelFunctions;
        private UnityBrowserLayerManagerFunctions _layerManagerFunctions;

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
            _webFunctions = new UnityBrowserWebFunctions(Browser);
            _searchFunctions = new UnityBrowserSearchFunctions(Browser);
            _userInterfaceFunctions = new UnityBrowserUserInterfaceFunctions(Browser);
            _terrainModelFunctions = new UnityBrowserTerrainModelFunctions(Browser);
            _layerManagerFunctions = new UnityBrowserLayerManagerFunctions(Browser);
        }

        protected override void OnBrowserLoad(JSONNode loadData) {
            _webFunctions.RegisterFunctions();
            _searchFunctions.RegisterFunctions();
            _userInterfaceFunctions.RegisterFunctions();
            _terrainModelFunctions.RegisterFunctions();
            _layerManagerFunctions.RegisterFunctions();
        }

        #region Controller event handlers

        private void TriggerClickedHandler(object sender, ClickedEventArgs e) {
            switch (CurrentActivity) {
                case ControllerModalActivity.Default:
                    SecondaryXRController controller = (SecondaryXRController)_controller;
                    controller.Flashlight?.Toggle();
                    break;
                default:
                    Input.RegisterKeyPress(KeyCode.T);
                    break;
            }
        }

        private void MenuButtonPressedHandler(object sender, ClickedEventArgs e) {
            MainModal mainModal = UserInterfaceManager.Instance.MainModal;
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            XRInteractableTerrain interactableTerrain = terrainModelManager.GetComponentFromCurrentModel<XRInteractableTerrain>();
            switch (CurrentActivity) {
                case ControllerModalActivity.Default:
                    // TODO Turn on secondary controller menu instead.
                    SecondaryXRController controller = (SecondaryXRController)_controller;
                    controller.Flashlight?.CycleNextColor();
                    break;
                case ControllerModalActivity.BBoxSelection:
                    interactableTerrain.CancelSelection();
                    if (interactableTerrain.CurrentActivity != XRInteractableTerrainActivity.BBoxSelection) {
                        mainModal.Visible = true;
                    }
                    break;
                case ControllerModalActivity.BookmarkResults:
                case ControllerModalActivity.NomenclatureResults:
                case ControllerModalActivity.ProductResults:
                    Input.RegisterKeyPress(KeyCode.M);
                    break;
                case ControllerModalActivity.LayerManager:
                    mainModal.Visible = true;
                    StartActivity(ControllerModalActivity.Default);
                    break;
                case ControllerModalActivity.ToolsDistance:
                    interactableTerrain.CancelSelection();
                    if (interactableTerrain.CurrentActivity != XRInteractableTerrainActivity.BBoxSelection) {
                        mainModal.Visible = true;
                    }
                    break;
                case ControllerModalActivity.ToolsProfile:
                    break;
                case ControllerModalActivity.ToolsSunAngle:
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
                    controller.WorldLights?.Brighten();
                } else {
                    controller.WorldLights?.Dim();
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
