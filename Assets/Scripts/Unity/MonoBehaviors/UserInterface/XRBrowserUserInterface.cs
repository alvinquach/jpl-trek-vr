using UnityEngine;

namespace TrekVRApplication {

    public abstract class XRBrowserUserInterface : BrowserUserInterface {

        protected MeshCollider _meshCollider;

        public override bool Visible {
            get { return _visible; }
            set { SetVisiblity(value, _meshRenderer, _meshCollider); }
        }

        public XRBrowser XRBrowser { get; protected set; }

        protected override void Init(Mesh mesh) {
            base.Init(mesh);

            _meshCollider = gameObject.AddComponent<MeshCollider>();
            _meshCollider.sharedMesh = mesh;

            XRBrowser = gameObject.AddComponent<XRBrowser>();
        }

        protected override void SetVisiblity(bool visible, params object[] objects) {
            XRBrowser.SetVisiblityState(true);
            base.SetVisiblity(visible, objects);
        }

    }

}
