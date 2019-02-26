using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public class MainModal : XRBrowserUserInterface {

        public const float AngleSweep = 180;
        public const float Height = 3.1f;
        public const float Radius = 3.1f;
        public const float Elevation = 0.069f;
        public const int Resolution = 720;

        private UnityBrowserWebFunctions _webFunctions;
        private UnityBrowserControllerFunctions _controllerFunctions;

        public override bool Visible {
            get {
                return _visible;
            }
            set {
                if (value) {
                    OpenModal();
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
            _controllerFunctions = new UnityBrowserControllerFunctions(_browser);
        }

        protected override void OnBrowserLoad(JSONNode loadData) {
            _webFunctions.RegisterFunctions();
            _controllerFunctions.RegisterFunctions();
        }

        private void OpenModal() {
            Camera eye = UserInterfaceManager.Instance.XRCamera;
            Ray forward = new Ray(eye.transform.position, Vector3.Scale(eye.transform.forward, new Vector3(1, 0, 1)));

            Vector3 menuPosition = eye.transform.position;
            menuPosition.y = MainModal.Elevation;
            transform.position = menuPosition;

            Vector3 menuOrientation = eye.transform.forward;
            menuOrientation.y = 0;
            transform.forward = menuOrientation;
        }

    }

}
