using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public abstract class BrowserUserInterface : MonoBehaviourWithTaskQueue {

        protected TaskStatus _initStatus = TaskStatus.NotStarted;

        protected MeshRenderer _meshRenderer;

        /// <summary>
        ///     Whether to set the browser game object to inactive after
        ///     Awake() is called. Browser game objects should start out
        ///     as active in order to get browser content load.
        /// </summary>
        public bool hideAfterInit = true;

        protected bool _visible;
        public virtual bool Visible {
            get {
                return _visible;
}
            set {
                _visible = value;
                Browser.EnableInput = value;
                Browser.EnableRendering = value;
                SetObjectsVisiblity(value, _meshRenderer);
            }
        }

        protected abstract GenerateMenuMeshTask GenerateMenuMeshTask { get; }

        protected abstract string DefaultUrl { get; }

        protected abstract int GetWidth();

        protected abstract int GetHeight();


        protected Browser _browser;
        public Browser Browser {
            get { return _browser; }
        }

        protected virtual void Awake() {
            Debug.Log(GenerateMenuMeshTask.GetType());
            GenerateMenuMeshTask?.Execute(meshData => {
                QueueTask(() => {
                    Mesh mesh = ProcessMeshData(meshData[0]);
                    Init(mesh);
                    if (hideAfterInit) {
                        Visible = false;
                    }
                    _initStatus = TaskStatus.Completed;

                });
            });
        }

        protected virtual void Init(Mesh mesh) {

            // This method can only be called once per instance.
            if (_initStatus != TaskStatus.NotStarted) {
                return;
            }

            _initStatus = TaskStatus.Started;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.material = UserInterfaceManager.Instance.UIMaterial;

            _browser = gameObject.AddComponent<Browser>();
            _browser.onLoad += OnBrowserLoad;
            _browser.Url = DefaultUrl;
            _browser.Resize(GetWidth(), GetHeight());

        }

        protected virtual Mesh ProcessMeshData(MeshData meshData) {

            Mesh mesh = new Mesh();

            // If needed, set the index format of the mesh to 32-bits,
            // so that the mesh can have more than 65k vertices.
            if (meshData.Vertices.Length > (1 << 16)) {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            mesh.vertices = meshData.Vertices;
            mesh.uv = meshData.TexCoords;
            mesh.triangles = meshData.Triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        protected virtual void OnBrowserLoad(JSONNode loadData) { }

        /// <summary>
        ///     For use by the 'Visible' property setter only. Do not call this method
        ///     outside of the setter. Set the visiblity value through the property instead.
        /// </summary>
        protected virtual void SetObjectsVisiblity(bool visible, params object[] objects) {

            // If visiblilty was set to false then hide the mesh renderer
            // and mesh collider immediately.
            if (!_visible) {
                foreach (object obj in objects) {
                    SetEnabled(obj, false);
                }
            }

            // If visiblilty was set to true, the mesh renderer and mesh 
            // collider need to be unhidden, but is delayed to give the
            // browser a chance re-render the contents first.
            else {
                // TODO Add variables to set the behavior of the browser
                // after unhiding (ie. whether to go back to root menu
                // or keep displaying same page).
                StartCoroutine(OnUnhide(objects));
            }
        }

        private IEnumerator OnUnhide(params object[] objects) {
            yield return new WaitForSeconds(0.1f); // TODO Fix magic number.
            foreach (object obj in objects) {
                SetEnabled(obj, true);
            }
        }

        private void SetEnabled(object obj, bool enabled) {
            if (obj == null) {
                return;
            }
            if (obj is Renderer) {
                ((Renderer) obj).enabled = enabled;
            }
            else if (obj is Collider) {
                ((Collider) obj).enabled = enabled;
            }
            else if (obj is Behaviour) {
                ((Behaviour)obj).enabled = enabled;
            }
        }

    }

}
