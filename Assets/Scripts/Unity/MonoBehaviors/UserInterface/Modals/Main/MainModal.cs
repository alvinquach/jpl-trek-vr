﻿using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class MainModal : XRBrowserUserInterface {

        public const float AngleSweep = 135;
        public const float Height = 0.69f;
        public const float Radius = 1.0f;
        public const int Resolution = 720;

        private UnityBrowserWebFunctions _webFunctions;
        private UnityBrowserSearchFunctions _searchFunctions;
        private UnityBrowserControllerFunctions _controllerFunctions;

        /// <summary>
        ///     <para>
        ///         Whether the main model is visible.
        ///     </para>
        ///     <para>
        ///         When the main modal's visiblity is set to true, then the mode of the
        ///         current visible model will automatically be set to Disabled.
        ///     </para>
        ///     <para>
        ///         However, when setting the main modal's visiblity to false, the mode of
        ///         the currently visible model must be set manaully.
        ///     </para>
        /// </summary>
        public override bool Visible {
            get {
                return _visible;
            }
            set {
                if (value) {
                    OpenModal();
                        
                    // FIXME Need to set the mode for all the terrain models, not just the current model.
                    // Or need to implement a way to inherit modes when switching to different models.
                    TerrainModel model = TerrainModelManager.Instance.CurrentVisibleModel;
                    if (model && model.GetType() == typeof(GlobalTerrainModel)) {
                        model.GetComponent<XRInteractablePlanet>()?.SwitchToMode(XRInteractablePlanetMode.Disabled);
                    } else {
                        model?.UseDisabledMaterial();
                    }
                }
                base.Visible = value;
            }
        }

        protected override string DefaultUrl { get; } = ZFBrowserConstants.BaseUrl;

        private GenerateCylindricalMenuMeshTask _generateMenuMeshTask;
        protected override GenerateMenuMeshTask GenerateMenuMeshTask {
            get { return _generateMenuMeshTask; }
        }

        protected override void Awake() {
            transform.localEulerAngles = new Vector3(0, 180);
            _generateMenuMeshTask = new GenerateCylindricalMenuMeshTask(AngleSweep, Height, Radius);
            base.Awake();
        }

        protected override int GetHeight() {
            return Resolution;
        }

        protected override int GetWidth() {
            return Mathf.RoundToInt(Mathf.PI * Radius * AngleSweep / 180 / Height * Resolution);
        }

        protected override void Init(Mesh mesh) {
            base.Init(mesh);
            _webFunctions = new UnityBrowserWebFunctions(_browser);
            _searchFunctions = new UnityBrowserSearchFunctions(_browser);
            _controllerFunctions = new UnityBrowserControllerFunctions(_browser);
        }

        protected override void OnBrowserLoad(JSONNode loadData) {
            _webFunctions.RegisterFunctions();
            _searchFunctions.RegisterFunctions();
            _controllerFunctions.RegisterFunctions();
        }

        public void NavigateToRootMenu() {
            ZFBrowserUtils.NavigateTo(Browser, $"{MainModalUrl}");
        }

        private void OpenModal() {
            Camera eye = UserInterfaceManager.Instance.XRCamera;
            Ray forward = new Ray(eye.transform.position, Vector3.Scale(eye.transform.forward, new Vector3(1, 0, 1)));

            Vector3 menuPosition = eye.transform.position;
            menuPosition.y -= Mathf.Clamp(Height / 2, 0, float.PositiveInfinity); // TODO Max value
            transform.position = menuPosition;

            Vector3 menuOrientation = eye.transform.forward;
            menuOrientation.y = 0;
            transform.forward = menuOrientation;
        }

    }

}