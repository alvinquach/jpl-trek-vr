using UnityEngine;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public abstract class ControllerModal : BrowserUserInterface {

        // Offsets relative to the controller
        private const float XOffset = 0.05f;
        private const float YOffset = 0;
        private const float ZOffset = 0.05f;

        public const float Width = 0.25f;
        public const float Height = 0.25f;
        public const int Resolution = 1024;

        protected XRController _controller;

        public abstract bool IsPrimary { get; }

        private GeneratePlanarMenuMeshTask _generateMenuMeshTask;
        protected override GenerateMenuMeshTask GenerateMenuMeshTask {
            get { return _generateMenuMeshTask; }
        }

        protected override string DefaultUrl { get; } = $"{BaseUrl}#{ControllerModalUrl}";

        public ControllerModalActivity CurrentActivity { get; private set; }

        protected override void Awake() {

            _controller = GetComponentInParent<XRController>();
            if (!_controller) {
                // TODO Throw exception
            }

            // Register event handlers
            _controller.OnMenuButtonClicked += MenuButtonClickedHandler;

            // Position the modal relative to the controller
            transform.localPosition = new Vector3((IsPrimary ? -1 : 1) * XOffset, YOffset, ZOffset);
            transform.localEulerAngles = new Vector3(-90, -180, 0);

            _generateMenuMeshTask = new GeneratePlanarMenuMeshTask(Width, Height,
                IsPrimary ? RelativePosition.Right : RelativePosition.Left);

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

        protected abstract void MenuButtonClickedHandler(object sender, ClickedEventArgs e);

        #endregion

        public virtual void StartActivity(ControllerModalActivity activity) {

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
                    terrainModelController.ShowDefaultPlanetModel(); // Hacky demo code
                    Debug.LogError($"Cannot swtich to {activity} activity; planet model is currently not visible.");
                    return;
                }
            }
            else if (activity == ControllerModalActivity.BookmarkResults || activity == ControllerModalActivity.ProductResults) {
                UserInterfaceManager.Instance.MainModal.Visible = false;
            }

            Visible = activity != ControllerModalActivity.Default;
            CurrentActivity = activity;
        }

    }
}