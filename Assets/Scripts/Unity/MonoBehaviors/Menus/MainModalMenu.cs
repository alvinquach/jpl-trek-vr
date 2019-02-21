using UnityEngine;

namespace TrekVRApplication {

    public class MainModalMenu : BrowserMenu {

        public const float AngleSweep = 180;
        public const float Height = 3.1f;
        public const float Radius = 3.1f;
        public const float Elevation = 0.069f;
        public const int Resolution = 720;

        protected override string RootUrl { get; } = "localGame://index.html";

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
    }

}
