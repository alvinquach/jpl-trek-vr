using UnityEngine;

namespace TrekVRApplication {

    public abstract class XRBrowserUserInterface : BrowserUserInterface {

        protected MeshCollider _meshCollider;

        public XRBrowser XRBrowser { get; protected set; }

        public override bool Visible {
            get {
                return _visible;
            }
            set {
                _visible = value;
                Browser.EnableInput = value;
                Browser.EnableRendering = value;
                XRBrowser.SetVisiblityState(value);
                SetObjectsVisiblity(value, _meshRenderer, _meshCollider);
            }
        }

        protected override void Init(Mesh mesh) {
            base.Init(mesh);

            _meshCollider = gameObject.AddComponent<MeshCollider>();
            _meshCollider.sharedMesh = mesh;

            XRBrowser = gameObject.AddComponent<XRBrowser>();
        }

    }

}
