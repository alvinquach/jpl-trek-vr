using System;
using UnityEngine;
using UnityEngine.Rendering;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    public abstract class TerrainModel : MonoBehaviourWithTaskQueue {

        [SerializeField]
        protected string _demFilePath;
        public string DemFilePath {
            get { return _demFilePath; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _demFilePath = value; }
        }

        [SerializeField]
        protected float _radius;
        public float Radius {
            get { return _radius; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _radius = value; }
        }

        [SerializeField]
        protected float _heightScale = 1.0f;
        public float HeightScale {
            get { return _heightScale; }
            // TODO Allow height scale to be changed after mesh is already generated.
            set { if (_initTaskStatus == TaskStatus.NotStarted) _heightScale = value; }
        }

        [SerializeField]
        protected int _baseDownsampleLevel = 2;

        public int BaseDownSampleLevel {
            get { return _baseDownsampleLevel; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _baseDownsampleLevel = value; }
        }

        // TODO Add option to use linear LOD downsampling.

        [SerializeField]
        protected int _lodLevels = 2;
        public int LodLevels {
            get { return _lodLevels; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _lodLevels = value; }
        }

        protected LOD[] _lods;

        private Material _material;
        public Material Material {
            get {
                return _material;
            }
            set {
                Transform lodContainer = transform.Find(GameObjectName.LODGroupContainer);
                Debug.Log($"{GameObjectName.LODGroupContainer} found = {lodContainer != null}");
                if (lodContainer) {
                    MeshRenderer[] meshRenderers = lodContainer.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer meshRenderer in meshRenderers) {
                        meshRenderer.material = value;
                    }
                }
                _material = value;
            }
        }

        protected TaskStatus _initTaskStatus = TaskStatus.NotStarted;

        /// <summary>
        ///     The last time this terrain was set to visible via the Visible property.
        /// </summary>
        public long LastVisible { get; private set; } = 0L;

        public bool Visible {
            get {
                return gameObject.activeSelf;
            }
            set {
                if (!value) {
                    gameObject.SetActive(false);
                }
                else if (!gameObject.activeSelf) {
                    LastVisible = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    gameObject.SetActive(true);
                }
            }
        }

        #region Unity lifecycle methods

        protected virtual void Awake() {
            TerrainModelManager.Instance.OnRenderModeChange += SetRenderMode;
        }

        // Start is used instead of Awake so that property values can 
        // be assigned before the model intialization starts.
        protected virtual void Start() {
            InitModel();
        }

        protected virtual void OnDestroy() {
            TerrainModelManager.Instance.OnRenderModeChange -= SetRenderMode;
        }

        #endregion

        // Can only be called once.
        public void InitModel() {
            if (_initTaskStatus > TaskStatus.NotStarted) {
                return;
            }
            _initTaskStatus = TaskStatus.Started;

            GenerateMaterial();
            GenerateMesh();
        }

        protected virtual void GenerateMaterial() {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            if (!terrainModelManager.BaseMaterial) {
                // TODO Throw exception.
            }
            _material = new Material(terrainModelManager.BaseMaterial);
            SetRenderMode(TerrainModelManager.Instance.TerrainRenderMode);
            // Population of the material's texture slots is up to the implementing class.
        }

        protected abstract void GenerateMesh();

        /// <summary>
        ///     <para>
        ///         Processes the generated mesh data. Creates a mesh renderer and mesh
        ///         filter for each LOD mesh, and assigns material to the mesh. Also creates
        ///         a LOD group containing the LOD mesh levels.
        ///     </para>
        ///     <para>
        ///         This method is intended to be called from implementations of TerrainModel;
        ///         it is not called by the TerrainModel abstract class itself.
        ///     </para>
        /// </summary>
        protected virtual void ProcessMeshData(MeshData[] meshData) {

            // Minimum base downsampling level should be 1.
            _baseDownsampleLevel = _baseDownsampleLevel < 1 ? 1 : _baseDownsampleLevel;

            // Add LOD group manager.
            // TODO Make this a class member variable.
            GameObject lodGroupContainer = new GameObject();
            lodGroupContainer.transform.SetParent(transform, false);
            lodGroupContainer.name = GameObjectName.LODGroupContainer;

            LODGroup lodGroup = lodGroupContainer.AddComponent<LODGroup>();
            LOD[] lods = new LOD[_lodLevels + 1];

            // Get Tiff file from the file path.

            // Create a child GameObject containing a mesh for each LOD level.
            for (int i = 0; i <= _lodLevels; i++) {

                GameObject child = new GameObject();
                child.transform.SetParent(lodGroupContainer.transform);

                // Name the LOD game object.
                child.name = $"LOD_{i}";

                // Use the parent's tranformations.
                child.transform.localPosition = Vector3.zero;
                child.transform.localScale = Vector3.one;
                child.transform.localEulerAngles = Vector3.zero;

                // Add MeshRenderer to child, and to the LOD group.
                MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
                lods[i] = new LOD(i == 0 ? 1 : Mathf.Pow(1 - (float)i / _lodLevels, 2), new Renderer[] { meshRenderer });

                // Add material to the MeshRenderer.
                if (_material != null) {
                    meshRenderer.material = _material;
                }

                MeshFilter meshFilter = child.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();

                // Assign mesh data
                MeshData lodMeshData = meshData[i];

                // If needed, set the index format of the mesh to 32-bits,
                // so that the mesh can have more than 65k vertices.
                if (lodMeshData.Vertices.Length > (1 << 16)) {
                    mesh.indexFormat = IndexFormat.UInt32;
                }

                float start = Time.realtimeSinceStartup;
                mesh.vertices = lodMeshData.Vertices;
                Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to assign vertices.");

                start = Time.realtimeSinceStartup;
                mesh.uv = lodMeshData.TexCoords;
                Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to assign UVs.");


                start = Time.realtimeSinceStartup;
                mesh.triangles = lodMeshData.Triangles;
                Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to assign triangles.");

                // This is a time consuming operation, and may cause the app to pause
                // for a couple of miliseconds since it runs on the main thread.
                start = Time.realtimeSinceStartup;
                mesh.RecalculateNormals();
                Debug.Log($"{child.name} took {Time.realtimeSinceStartup - start} seconds to recalculate normals.");

                meshFilter.mesh = mesh;

            }

            // Assign LOD meshes to LOD group.
            lodGroup.SetLODs(lods);

            // Calculate bounds if there are one or more LOD level. If there are no LOD levels, 
            // then we can just disable LOD, so there is no need to calculate bounds.
            if (_lodLevels > 0) {
                lodGroup.RecalculateBounds();
            }
            else {
                lodGroup.enabled = false;
            }

        }

        protected virtual TerrainModelMetadata GenerateTerrainModelMetadata() {
            return new TerrainModelMetadata() {
                demFilePath = _demFilePath,
                radius = _radius * TerrainModelScale,
                heightScale = _heightScale * TerrainModelScale,
                lodLevels = _lodLevels,
                baseDownsample = _baseDownsampleLevel
            };
        }

        protected virtual void SetRenderMode(bool enabled) {
            string shaderName = enabled ? "Custom/MultiDiffuseShader" : "Custom/MultiDiffuseTransparentShader";
            SwitchToShader(shaderName);
            if (!enabled) {
                Material.SetFloat("_Opacity", 0.5f);
            }
        }

        private void SwitchToShader(string shaderName) {
            Shader shader = Shader.Find(shaderName);
            if (shader) {
                Material.shader = shader;
            } else {
                Debug.LogError($"Could not find shader {shaderName}.");
            }
        }

    }

}
