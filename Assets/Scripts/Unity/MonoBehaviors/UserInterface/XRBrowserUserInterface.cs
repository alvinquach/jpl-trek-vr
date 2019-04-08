using UnityEngine;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public abstract class XRBrowserUserInterface : BrowserUserInterface {

        protected MeshCollider _meshCollider;

        public XRBrowser XRBrowser { get; protected set; }

        public override bool Visible {
            get => _visible;
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

        /// <summary>
        ///     Optional event handler that can be registered to the TerrainModelManager's
        ///     OnCurrentTerrainModelChange event emitter by the implementing class.
        /// </summary>
        protected void OnTerrainModelChange(TerrainModel terrainModel) {
            string terrainType = terrainModel is GlobeTerrainModel ? "globe" : "local";
            Browser.EvalJS($"{AngularInjectableContainerPath}.{TerrainModelServiceName}.currentTerrainType = '{terrainType}';");
        }

    }

}
