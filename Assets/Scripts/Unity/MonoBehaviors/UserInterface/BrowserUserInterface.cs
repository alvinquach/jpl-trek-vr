using UnityEngine;
using UnityEngine.Rendering;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public abstract class BrowserUserInterface : MonoBehaviourWithTaskQueue {

        protected TaskStatus _initStatus = TaskStatus.NotStarted;

        protected abstract GenerateMenuMeshTask GenerateMenuMeshTask { get; }

        protected abstract string RootUrl { get; }

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

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = UserInterfaceManager.Instance.UIMaterial;

            _browser = gameObject.AddComponent<Browser>();
            _browser.onLoad += OnBrowserLoad;
            _browser.Url = RootUrl;
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

    }

}
