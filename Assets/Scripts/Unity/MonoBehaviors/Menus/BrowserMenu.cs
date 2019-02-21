using UnityEngine;
using UnityEngine.Rendering;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public abstract class BrowserMenu : MonoBehaviourWithTaskQueue {

        public XRBrowser XRBrowser { get; protected set; }

        protected TaskStatus _initStatus = TaskStatus.NotStarted;

        protected abstract GenerateMenuMeshTask GenerateMenuMeshTask { get; }

        protected abstract string RootUrl { get; }

        protected abstract int GetWidth();

        protected abstract int GetHeight();

        protected virtual void Awake() {
            Debug.Log(GenerateMenuMeshTask.GetType());
            GenerateMenuMeshTask?.Execute(meshData => {
                QueueTask(() => {
                    Mesh mesh = ProcessMeshData(meshData[0]);
                    Init(mesh);
                });
            });
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

        protected virtual void Init(Mesh mesh) {


            // This method can only be called once per instance.
            if (_initStatus != TaskStatus.NotStarted) {
                return;
            }

            _initStatus = TaskStatus.Started;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = UserInterfaceManager.Instance.UIMaterial;

            Browser browser = gameObject.AddComponent<Browser>();
            browser.Url = RootUrl;
            browser.Resize(GetWidth(), GetHeight());

            XRBrowser = gameObject.AddComponent<XRBrowser>();

            _initStatus = TaskStatus.Completed;
        }

    }

}
