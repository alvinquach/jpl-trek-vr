using System;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrekVRApplication {

    public abstract class TerrainModel : MonoBehaviourWithTaskQueue {

        [SerializeField]
        protected string _demFilePath;
        public string DemFilePath {
            get { return _demFilePath; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _demFilePath = value; }
        }

        [SerializeField]
        protected string _albedoFilePath;
        public string AlbedoFilePath {
            get { return _albedoFilePath; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _albedoFilePath = value; }
        }

        public abstract float HeightScale { get; set; }
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

        public virtual Texture2D Albedo { get; protected set; }

        protected virtual Material EnabledMaterial { get; set; }

        protected virtual Material DisabledMaterial { get; set; }

        private Material _currentMaterial;
        public Material CurrentMaterial {
            get {
                return _currentMaterial;
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
                _currentMaterial = value;
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

        // Start is used instead of Awake so that property values can 
        // be assigned before the model intialization starts.
        protected virtual void Start() {
            InitModel();
            if (UserInterfaceManager.Instance.MainModal.Visible) {
                UseDisabledMaterial();
            } else {
                UseEnabledMaterial();
            }
        }

        // If this method is overriden, then this method should be 
        // called from the overriding method.
        protected override void Update() {
            base.Update();
        }

        #endregion

        // Can only be called once.
        public virtual void InitModel() {
            if (_initTaskStatus > TaskStatus.NotStarted) {
                return;
            }
            _initTaskStatus = TaskStatus.Started;
            GenerateTerrainMeshTask generateMeshTask = InstantiateGenerateMeshTask();
            generateMeshTask.Execute((meshData) => {
                QueueTask(() => ProcessMeshData(meshData));
                _initTaskStatus = TaskStatus.Completed;
            });
        }

        /// <summary>Instantiates the task for generating the terrain model mesh data.</summary>
        protected abstract GenerateTerrainMeshTask InstantiateGenerateMeshTask();

        protected virtual void ProcessMeshData(MeshData[] meshData) {

            GenerateMaterials();
            _currentMaterial = UserInterfaceManager.Instance.MainModal.Visible ? DisabledMaterial : EnabledMaterial;

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
                if (_currentMaterial != null) {
                    meshRenderer.material = _currentMaterial;
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

        protected virtual void GenerateMaterials() {

            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            if (!terrainModelManager.BaseEnabledMaterial || !terrainModelManager.BaseDisabledMaterial) {
                // TODO Throw exception.
            }
            EnabledMaterial = new Material(terrainModelManager.BaseEnabledMaterial);
            DisabledMaterial = new Material(terrainModelManager.BaseDisabledMaterial);

            // TODO Also check if file exists.
            if (!string.IsNullOrEmpty(_albedoFilePath)) {

                float start = Time.realtimeSinceStartup;

                // Task for loading texture data on a separate thread.
                LoadBGRAImageFromFileTask loadImageTask = new LoadBGRAImageFromFileTask(_albedoFilePath);

                // Execute the task.
                loadImageTask.Execute(image => {

                    int width = loadImageTask.TextureWidth, height = loadImageTask.TextureHeight;

                    // Queue a task to apply texture on the main thread during the next update.
                    QueueTask(() => {

                        TextureCompressionFormat format = TextureCompressionFormat.UncompressedWithAlpha;

                        //byte[] data = TextureUtils.GenerateMipmaps(image);
                        byte[] data = new byte[TextureUtils.ComputeTextureSize(width, height, format)];
                        image.CopyRawData(data);

                        Debug.Log($"Took {Time.realtimeSinceStartup - start} seconds to generate texture.");
                        start = Time.realtimeSinceStartup;

                        Albedo = new Texture2D(width, height, format.GetUnityFormat(), true);
                        Albedo.GetRawTextureData<byte>().CopyFrom(data);
                        Albedo.Apply(true);

                        // Set albedo texture to both the enabled and disabled materials.
                        EnabledMaterial.SetTexture("_MainTex", Albedo);
                        DisabledMaterial.SetTexture("_MainTex", Albedo);

                        Debug.Log($"Took {Time.realtimeSinceStartup - start} seconds to apply texture.");
                    });

                });

            }

        }

        protected abstract TerrainModelMetadata GenerateMetadata();

        public void UseEnabledMaterial() {
            Debug.Log("SWITCHING TO ENABLED MATERIAL");
            CurrentMaterial = EnabledMaterial;
        }

        public void UseDisabledMaterial() {
            Debug.Log("SWITCHING TO DISABLED MATERIAL");
            CurrentMaterial = DisabledMaterial;
        }

    }

}
