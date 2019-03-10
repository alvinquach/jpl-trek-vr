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

        public override bool Visible {
            get {
                return base.Visible;
            }
            set {
                base.Visible = value;
                if (Input) {
                    Input.SetVisiblityState(value);
                }
            }
        }

        private GeneratePlanarMenuMeshTask _generateMenuMeshTask;
        protected override GenerateMenuMeshTask GenerateMenuMeshTask {
            get { return _generateMenuMeshTask; }
        }

        protected override string DefaultUrl { get; } = $"{BaseUrl}#{ControllerModalUrl}";

        public ControllerModalInput Input { get; private set; }

        public ControllerModalActivity CurrentActivity { get; private set; }

        protected override void Awake() {
            _controller = GetComponentInParent<XRController>();

            // Position the modal relative to the controller
            transform.localPosition = new Vector3((IsPrimary ? -1 : 1) * XOffset, YOffset, ZOffset);
            transform.localEulerAngles = new Vector3(-90, -180, 0);

            _generateMenuMeshTask = new GeneratePlanarMenuMeshTask(Width, Height,
                IsPrimary ? RelativePosition.Right : RelativePosition.Left);

            base.Awake();
        }

        protected override void Init(Mesh mesh) {
            base.Init(mesh);
            Input = Browser.gameObject.AddComponent<ControllerModalInput>();
        }

        protected override int GetHeight() {
            return Resolution;
        }

        protected override int GetWidth() {
            return Mathf.RoundToInt(Width / Height * Resolution);
        }

        public virtual void StartActivity(ControllerModalActivity activity) {

            ZFBrowserUtils.NavigateTo(Browser, activity.GetModalUrl());

            TerrainModelManager terrainModelController = TerrainModelManager.Instance;
            XRInteractablePlanet planet;

            // Switch away from current activity.
            // TODO Convert this to switch case.
            if (CurrentActivity == ControllerModalActivity.BBoxSelection) {
                planet = terrainModelController.GetComponentFromCurrentModel<XRInteractablePlanet>();
                planet.SwitchToMode(XRInteractablePlanetMode.Navigate);
            }


            // Switch to new acivity.
            // TODO Convert this to switch case.
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    if (terrainModelController.GlobalPlanetModelIsVisible()) {

                        // FIXME Need to set the mode for all the terrain models, not just the planet.
                        planet = terrainModelController.GetComponentFromCurrentModel<XRInteractablePlanet>();
                        planet.SwitchToMode(XRInteractablePlanetMode.Select);

                        UserInterfaceManager.Instance.MainModal.Visible = false;
                    }
                    else {
                        terrainModelController.ShowGlobalPlanetModel(); // Hacky demo code
                        //Debug.LogError($"Cannot swtich to {activity} activity; planet model is currently not visible.");
                        return;
                    }
                    break;

                case ControllerModalActivity.BookmarkResults:
                case ControllerModalActivity.ProductResults:
                case ControllerModalActivity.LayerManager:

                    // FIXME Need to set the mode for all the terrain models, not just the planet.
                    planet = terrainModelController.GetComponentFromCurrentModel<XRInteractablePlanet>();
                    planet.SwitchToMode(XRInteractablePlanetMode.Select);

                    UserInterfaceManager.Instance.MainModal.Visible = false;
                    break;
            }

            Visible = activity != ControllerModalActivity.Default;
            CurrentActivity = activity;
        }

    }
}