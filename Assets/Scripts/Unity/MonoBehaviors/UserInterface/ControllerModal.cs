using UnityEngine;

namespace TrekVRApplication {

    public class ControllerModal : BrowserUserInterface {

        // Offsets relative to the controller
        private const float XOffset = 0.05f;
        private const float YOffset = 0;
        private const float ZOffset = 0.05f;

        public const float Width = 0.25f;
        public const float Height = 0.25f;
        public const int Resolution = 1024;

        private XRController _controller;
        private bool _isPrimary;

        private GeneratePlanarMenuMeshTask _generateMenuMeshTask;
        protected override GenerateMenuMeshTask GenerateMenuMeshTask {
            get { return _generateMenuMeshTask; }
        }

        protected override string DefaultUrl { get; } = $"{ZFBrowserConstants.BaseUrl}/#{ControllerModalActivity.Default.GetModalUrl()}";

        public ControllerModalActivity CurrentActivity { get; private set; }

        protected override void Awake() {

            _controller = GetComponentInParent<XRController>();
            if (!_controller) {
                // TODO Throw exception
            }
            _isPrimary = _controller.GetType() == typeof(PrimaryXRController);

            // Register event handlers
            _controller.OnMenuButtonClicked += MenuButtonClickedHandler;

            // Position the modal relative to the controller
            transform.localPosition = new Vector3((_isPrimary ? 1 : -1) * XOffset, YOffset, ZOffset);
            transform.localEulerAngles = new Vector3(-90, -180, 0);

            _generateMenuMeshTask = new GeneratePlanarMenuMeshTask(Width, Height,
                _isPrimary ? RelativePosition.Left : RelativePosition.Right);

            base.Awake();
        }

        private void OnDestroy() {
            _controller.OnMenuButtonClicked -= MenuButtonClickedHandler;
        }

        protected override int GetHeight() {
            return Resolution;
        }

        protected override int GetWidth() {
            return Mathf.RoundToInt(Width / Height * Resolution);
        }

        #region Controller event handlers

        private void MenuButtonClickedHandler(object sender, ClickedEventArgs e) {
            if (CurrentActivity == ControllerModalActivity.Default) {
                MainModal mainModal = UserInterfaceManager.Instance.MainModal;
                mainModal.Visible = !mainModal.Visible;
            }
        }

        #endregion

        public void StartActivity(ControllerModalActivity activity) {
            if (activity == CurrentActivity) {
                return;
            }
            if (_isPrimary && activity.IsSecondaryOnly()) {
                Debug.LogError($"Activity {activity} is only available for the secondary controller.");
                return;
            }
            if (!_isPrimary && activity.IsPrimaryOnly()) {
                Debug.LogError($"Activity {activity} is only available for the primary controller.");
                return;
            }

            ZFBrowserUtils.NavigateTo(Browser, activity.GetModalUrl());

            // Switch away from current activity.
            if (CurrentActivity == ControllerModalActivity.BBoxSelection) {
                TerrainModelManager terrainModelController = TerrainModelManager.Instance;
                XRInteractablePlanet planet = terrainModelController.GetComponentFromCurrentModel<XRInteractablePlanet>();
                planet.InteractionMode = XRInteractablePlanetMode.Navigate;
            }

            // Switch to new acivity.
            if (activity == ControllerModalActivity.BBoxSelection) {
                TerrainModelManager terrainModelController = TerrainModelManager.Instance;
                if (terrainModelController.DefaultPlanetModelIsVisible()) {
                    XRInteractablePlanet planet = terrainModelController.GetComponentFromCurrentModel<XRInteractablePlanet>();
                    planet.InteractionMode = XRInteractablePlanetMode.Select;
                    UserInterfaceManager.Instance.MainModal.Visible = false;
                } else {
                    Debug.LogError($"Cannot swtich to {activity} activity; planet model is currently not visible.");
                    return;
                }
            }

            Visible = activity != ControllerModalActivity.Default;
            CurrentActivity = activity;
        }

    }
}