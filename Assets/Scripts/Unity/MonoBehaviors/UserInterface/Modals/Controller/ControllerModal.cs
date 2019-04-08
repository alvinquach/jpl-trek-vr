using UnityEngine;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public abstract class ControllerModal : BrowserUserInterface {

        // Offsets relative to the controller
        private const float XOffset = 0.05f;
        private const float YOffset = 0;
        private const float ZOffset = 0.05f;

        /// <summary>
        ///     Width of the modal in world units.
        /// </summary>
        private const float WorldWidth = 0.25f;

        /// <summary>
        ///     Height of the modal in world units.
        /// </summary>
        private const float WorldHeight = 0.25f;

        /// <summary>
        ///     Vertical resolution of the modal in pixels.
        /// </summary>
        private const int Resolution = 1024;

        protected override int Width => Mathf.RoundToInt(WorldWidth / WorldHeight * Resolution);

        protected override int Height => Resolution;

        protected override bool RegisterToUserInterfaceManager => false;

        protected XRController _controller;

        public abstract bool IsPrimary { get; }

        public override bool Visible {
            get => base.Visible;
            set {
                base.Visible = value;
                if (Input) {
                    Input.SetVisiblityState(value);
                }
            }
        }

        private GeneratePlanarMenuMeshTask _generateMenuMeshTask;
        protected override GenerateMenuMeshTask GenerateMenuMeshTask => _generateMenuMeshTask;

        protected override string DefaultUrl => $"{BaseUrl}#{ControllerModalUrl}";

        public ControllerModalInput Input { get; private set; }

        public ControllerModalActivity CurrentActivity { get; private set; }

        protected override void Awake() {
            _controller = GetComponentInParent<XRController>();

            // Position the modal relative to the controller
            transform.localPosition = new Vector3((IsPrimary ? -1 : 1) * XOffset, YOffset, ZOffset);
            transform.localEulerAngles = new Vector3(-90, -180, 0);

            _generateMenuMeshTask = new GeneratePlanarMenuMeshTask(WorldWidth, WorldHeight,
                IsPrimary ? RelativePosition.Right : RelativePosition.Left);

            base.Awake();
        }

        protected override void Init(Mesh mesh) {
            base.Init(mesh);
            Input = Browser.gameObject.AddComponent<ControllerModalInput>();
        }

        public virtual void StartActivity(ControllerModalActivity activity) {

            string modalUrl = activity.GetModalUrl();

            TerrainModelManager terrainModelController = TerrainModelManager.Instance;
            XRInteractableGlobeTerrain globe;

            // Switch to new acivity.
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    TerrainModel terrainModel = terrainModelController.CurrentVisibleModel;
                    XRInteractableTerrain interactableTerrain = terrainModel.InteractionController;
                    interactableTerrain.SwitchToActivity(XRInteractableTerrainActivity.BBoxSelection);
                    UserInterfaceManager.Instance.MainModal.Visible = false;
                    break;
                case ControllerModalActivity.BookmarkResults:
                case ControllerModalActivity.ProductResults:
                    int? searchListActiveIndex = TrekSearchWebService.Instance.SearchListActiveIndex;
                    Debug.Log(searchListActiveIndex == null ? "null" : searchListActiveIndex.ToString());
                    if (searchListActiveIndex != null) {
                        modalUrl += $"/{searchListActiveIndex}";
                    }
                    goto case ControllerModalActivity.LayerManager;
                case ControllerModalActivity.LayerManager:

                    // FIXME Need to set the mode for all the terrain models, not just the planet.
                    globe = terrainModelController.GetComponentFromCurrentModel<XRInteractableGlobeTerrain>();
                    globe.SwitchToActivity(XRInteractableTerrainActivity.Default);

                    UserInterfaceManager.Instance.MainModal.Visible = false;
                    break;
            }

            ZFBrowserUtils.NavigateTo(Browser, modalUrl);

            Visible = activity != ControllerModalActivity.Default;
            CurrentActivity = activity;
        }

    }
}