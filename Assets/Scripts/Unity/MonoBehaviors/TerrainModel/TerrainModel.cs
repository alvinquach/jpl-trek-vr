using System;
using UnityEngine;
using UnityEngine.Rendering;
using static TrekVRApplication.TerrainModelConstants;
using static TrekVRApplication.FlagUtils;

namespace TrekVRApplication {

    public abstract class TerrainModel : MonoBehaviourWithTaskQueue {

        protected GameObject _lodGroupContainer;
        public abstract XRInteractableTerrain InteractionController { get; }

        [SerializeField]
        protected string _demFilePath;
        public string DemFilePath {
            get => _demFilePath;
            set { if (_initTaskStatus == TaskStatus.NotStarted) _demFilePath = value; }
        }

        [SerializeField]
        protected float _radius;
        public float Radius {
            get => _radius;
            set { if (_initTaskStatus == TaskStatus.NotStarted) _radius = value; }
        }

        [SerializeField]
        protected float _heightScale = 1.0f;
        public float HeightScale {
            get => _heightScale;
            // TODO Allow height scale to be changed after mesh is already generated.
            set { if (_initTaskStatus == TaskStatus.NotStarted) _heightScale = value; }
        }

        // TODO Add option to use linear LOD downsampling.

        [SerializeField]
        protected int _lodLevels = 1;
        /// <summary>
        ///     The number of LOD levels to be generated, excluding LOD 0 and physics LOD.
        /// </summary>
        public int LodLevels {
            get => _lodLevels;
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    // Number of LOD levels must be a non-negative integer.
                    _lodLevels = MathUtils.Clamp(value, 0);
                }
            }
        }

        [SerializeField]
        protected int _baseDownsampleLevel = 0;
        /// <summary>
        ///     <para>
        ///         The amount of downsampling applied to the DEM file to generate the 
        ///         mesh (LOD 0). The actual amount of downsampling applied is 2^value.
        ///     </para>
        ///     <para>
        ///         For example, a value of 0 will have no downsampling, while a value
        ///         of 3 will downsample the DEM image by a factor of 8.
        ///     </para>
        /// </summary>
        public int BaseDownsampleLevel {
            get => _baseDownsampleLevel;
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    // Downsampling level must be a non-negative integer.
                    _baseDownsampleLevel = MathUtils.Clamp(value, 0);
                }
            }
        }

        [SerializeField]
        protected int _physicsDownsampleLevel = -1;
        /// <summary>
        ///     <para>
        ///         The amount of downsampling applied to the DEM file to generate the 
        ///         physics mesh. The actual amount of downsampling applied is 2^value.
        ///     </para>
        ///     <para>
        ///         For example, a value of 0 will have no downsampling, while a value
        ///         of 3 will downsample the DEM image by a factor of 8.
        ///     </para>
        ///     <para>
        ///         Set this to a negative number to indicate that a physics mesh does
        ///         not need to be generated.
        ///     </para>
        /// </summary>
        public int PhysicsDownsampleLevel {
            get => _physicsDownsampleLevel;
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    _physicsDownsampleLevel = value;
                }
            }
        }

        private Material _material;
        public Material Material {
            get => _material;
            set {
                _material = value;
                UpdateMeshRendererMaterials();
            }
        }

        public bool EnableOverlay {
            get => ContainsFlag((int)RenderMode, (int)TerrainModelRenderMode.Overlay);
            set => RenderMode = (TerrainModelRenderMode)AddOrRemoveFlag((int)RenderMode, (int)TerrainModelRenderMode.Overlay, value);
        }

        public bool UseDisabledMaterial {
            get => ContainsFlag((int)RenderMode, (int)TerrainModelRenderMode.Disabled);
            set => RenderMode = (TerrainModelRenderMode)AddOrRemoveFlag((int)RenderMode, (int)TerrainModelRenderMode.Disabled, value);
        }

        private TerrainModelRenderMode _renderMode;
        public TerrainModelRenderMode RenderMode {
            get => _renderMode;
            private set {
                _renderMode = value;
                UpdateMaterialProperties();
            }
        }

        protected TaskStatus _initTaskStatus = TaskStatus.NotStarted;

        /// <summary>
        ///     The last time this terrain was set to visible via the Visible property.
        /// </summary>
        public long LastVisible { get; private set; } = 0L;

        public bool Visible {
            get => gameObject.activeSelf;
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
        }

        protected virtual void OnDestroy() {
            Destroy(_material);
            // TODO Destroy more thisngs
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

            TerrainModelOverlayController terrainModelOverlayController = TerrainModelOverlayController.Instance;
            if (terrainModelOverlayController) {
                _material.SetTexture("_Overlay", terrainModelOverlayController.RenderTexture);
            }

            UseDisabledMaterial = !TerrainModelManager.Instance.TerrainInteractionEnabled;
            // Population of the material's texture slots is up to the implementing class.
        }

        protected abstract void GenerateMesh();

        /// <summary>
        ///     <para>
        ///         Processes the generated mesh data. Creates a mesh renderer and mesh
        ///         filter for each LOD mesh, and assigns material to the mesh. Also creates
        ///         a LOD group containing the LOD mesh levels. Note that this method does
        ///         not process the physics mesh (if one has been generated) — it is up to
        ///         the implementing class to add logic to process the phycics mesh.
        ///     </para>
        ///     <para>
        ///         This method is intended to be called from implementations of TerrainModel;
        ///         it is not called by the TerrainModel abstract class itself.
        ///     </para>
        ///     <para>
        ///         After this is called, the member variable _lodGroupContainer will be accessible.
        ///     </para>
        /// </summary>
        protected virtual void ProcessMeshData(MeshData[] meshData) {

            // Add LOD group manager.
            _lodGroupContainer = new GameObject(GameObjectName.LODGroupContainer) {
                layer = (int)CullingLayer.Terrain
            };
            _lodGroupContainer.transform.SetParent(transform, false);

            LODGroup lodGroup = _lodGroupContainer.AddComponent<LODGroup>();
            LOD[] lods = new LOD[_lodLevels + 1];

            // Get Tiff file from the file path.

            // Create a child GameObject containing a mesh for each LOD level.
            for (int i = 0; i <= _lodLevels; i++) {

                GameObject child = new GameObject($"LOD_{i}") {
                    layer = (int)CullingLayer.Terrain
                };
                child.transform.SetParent(_lodGroupContainer.transform);

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

                // Create a Mesh from the mesh data and add the mesh to a MeshFilter.
                Mesh mesh = ConvertToMesh(meshData[i], child.name);
                child.AddComponent<MeshFilter>().mesh = mesh;
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

        protected Mesh ConvertToMesh(MeshData meshData, string label = "ProcessMeshData()") {
            Mesh mesh = new Mesh();

            // If needed, set the index format of the mesh to 32-bits,
            // so that the mesh can have more than 65k vertices.
            if (meshData.Vertices.Length > (1 << 16)) {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            float start = Time.realtimeSinceStartup;
            mesh.vertices = meshData.Vertices;
            Debug.Log($"{label} took {Time.realtimeSinceStartup - start} seconds to assign vertices.");

            start = Time.realtimeSinceStartup;
            mesh.uv = meshData.TexCoords;
            Debug.Log($"{label} took {Time.realtimeSinceStartup - start} seconds to assign UVs.");


            start = Time.realtimeSinceStartup;
            mesh.triangles = meshData.Triangles;
            Debug.Log($"{label} took {Time.realtimeSinceStartup - start} seconds to assign triangles.");

            // This is a time consuming operation, and may cause the app to pause
            // for a couple of miliseconds since it runs on the main thread.
            start = Time.realtimeSinceStartup;
            mesh.RecalculateNormals();
            Debug.Log($"{label} took {Time.realtimeSinceStartup - start} seconds to recalculate normals.");

            return mesh;
        }

        /** Helper method for added a shadow casting mesh to the terrain. */
        protected GameObject AddShadowCaster(Mesh mesh) {
            GameObject shadowCaster = new GameObject(GameObjectName.TerrainShadowCaster) {
                layer = (int)CullingLayer.TerrainShadowCaster
            };
            shadowCaster.transform.SetParent(transform, false);
            MeshRenderer meshRenderer = shadowCaster.AddComponent<MeshRenderer>();
            meshRenderer.material = Material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            meshRenderer.receiveShadows = false;
            MeshFilter meshFilter = shadowCaster.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            return shadowCaster;
        }

        protected virtual TerrainModelMeshMetadata GenerateMeshMetadata() {
            return new TerrainModelMeshMetadata() {
                DemFilePath = DemFilePath,
                Radius = Radius * TerrainModelScale,
                HeightScale = HeightScale * TerrainModelScale,
                LodLevels = LodLevels,
                BaseDownsample = BaseDownsampleLevel,
                PhysicsDownsample = PhysicsDownsampleLevel
            };
        }

        /// <summary>
        ///     Update the material properties to reflect current render mode.
        /// </summary>
        private void UpdateMaterialProperties() {
            string shaderName = UseDisabledMaterial ? "MultiDiffuseTransparent" :
                EnableOverlay ? "MultiDiffuseOverlayMultipass" : "MultiDiffuse";

            SwitchToShader($"Custom/Terrain/{shaderName}");

            // TODO Define the opacity as a constant.
            Material.SetFloat("_DiffuseOpacity", UseDisabledMaterial ? 0.5f : 1); 

            // This is redundant, since the overlay shader is not used unless
            // the overlay is enabled and the interaction is not disabled.
            Material.SetFloat("_OverlayOpacity", EnableOverlay && !UseDisabledMaterial ? 1 : 0); 
        }

        private void SwitchToShader(string shaderName) {
            Shader shader = Shader.Find(shaderName);
            if (shader) {
                Material.shader = shader;
            } else {
                Debug.LogError($"Could not find shader {shaderName}.");
            }
        }

        private void UpdateMeshRendererMaterials() {
            if (!_lodGroupContainer) {
                return;
            }

            MeshRenderer[] meshRenderers = _lodGroupContainer.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers) {
                //meshRenderer.materials = materials;
                meshRenderer.material = Material;
            }
        }

    }

}
