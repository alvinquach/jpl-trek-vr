using UnityEngine;
using UnityEngine.Rendering;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public abstract class XRBrowserUserInterface : BrowserUserInterface {

        public XRBrowser XRBrowser { get; protected set; }

        protected override void Init(Mesh mesh) {
            base.Init(mesh);
            XRBrowser = gameObject.AddComponent<XRBrowser>();
        }

    }

}
