using System;
using UnityEngine;

namespace TrekVRApplication {

    public class ControllerModal : BrowserUserInterface {

        // Offsets relative to the controller
        private const float XOffset = 0.05f;
        private const float YOffset = 0;
        private const float ZOffset = -0.05f;

        public const float Width = 0.2f;
        public const float Height = 0.2f;
        public const int Resolution = 1024;

        private XRController _controller;

        private GeneratePlanarMenuMeshTask _generateMenuMeshTask;
        protected override GenerateMenuMeshTask GenerateMenuMeshTask {
            get { return _generateMenuMeshTask; }
        }

        private string _rootUrl;
        protected override string RootUrl {
            get { return _rootUrl; }
        }

        protected override void Awake() {

            _controller = GetComponentInParent<XRController>();
            if (!_controller) {
                // TODO Throw exception
            }
            bool isPrimary = _controller.GetType() == typeof(PrimaryXRController);
            
            _rootUrl = isPrimary ? "localhost:4200" : "localhost:4200"; // TODO Set this properly.

            // Position the modal relative to the controller
            transform.localPosition = new Vector3((isPrimary ? 1 : -1) * XOffset, YOffset, ZOffset);
            transform.localEulerAngles = new Vector3(-90, -180, 0);

            _generateMenuMeshTask = new GeneratePlanarMenuMeshTask(Width, Height, 
                isPrimary ? RelativePosition.Left : RelativePosition.Right);

            base.Awake();
        }

        protected override int GetHeight() {
            return Resolution;
        }

        protected override int GetWidth() {
            return Mathf.RoundToInt(Width / Height * Resolution);
        }

    }

}
