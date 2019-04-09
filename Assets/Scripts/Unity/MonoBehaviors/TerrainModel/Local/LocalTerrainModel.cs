using UnityEngine;
using static TrekVRApplication.ServiceManager;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    [RequireComponent(typeof(XRInteractableLocalTerrain))]
    public sealed class LocalTerrainModel : TerrainModel {

        // TODO Move these to constants file.
        private const float ViewTransitionDuration = 1.6f;
        private const float TableWidth = 2.0f;
        private const float TableHeight = 0.5f;
        private const float TableTopGap = 0.01f;

        public override XRInteractableTerrain InteractionController => GetComponent<XRInteractableLocalTerrain>();

        private bool _animateOnInitialization;
        public bool AnimateOnInitialization {
            get => _animateOnInitialization;
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    _animateOnInitialization = value;
                }
            }
        }

        private bool _useTemporaryBaseTextures = true;
        public bool UseTemporaryBaseTextures {
            get => _useTemporaryBaseTextures;
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    _useTemporaryBaseTextures = value;
                }
            }
        }

        [SerializeField]
        private string _demUUID;
        public override string DemUUID {
            get => _demUUID;
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    _demUUID = value;
                }
            }
        }

        // TODO Add property to include other product layers.

        private BoundingBox _boundingBox;
        public BoundingBox BoundingBox {
            get => _boundingBox;
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    _boundingBox = value;
                    SquareBoundingBox = BoundingBoxUtils.ExpandToSquare(value);
                }
            }
        }

        public BoundingBox SquareBoundingBox { get; private set; }

        private TaskStatus _generateDetailedMeshTaskStatus = TaskStatus.NotStarted;

        private TaskStatus _viewTransitionTaskStatus = TaskStatus.NotStarted;
        private float _viewTransitionProgress = 0.0f;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Quaternion _startRotation;
        private Quaternion _targetRotation;
        private float _targetScale = 1f;

        #region Unity lifecycle methods

        protected override void Start() {
            base.Start();
            // TODO Call method to download DEM and textures here.
        }

        protected override void Update() {
            base.Update();
            if (_viewTransitionTaskStatus == TaskStatus.InProgress) {
                TransitionView();
            }
        }

        protected override void OnDestroy() {
            // TODO Unregister all layers
            TerrainModelProductMetadata texInfo = GenerateProductMetadata(GlobalMosaicUUID);
            TerrainModelTextureManager.Instance.RegisterUsage(texInfo, false);
        }

        #endregion

        protected override void GenerateMaterial() {
            base.GenerateMaterial();

            TerrainOverlayController terrainModelOverlayController = LocalTerrainOverlayController.Instance;
            if (terrainModelOverlayController) {
                Material.SetTexture("_Overlay", terrainModelOverlayController.RenderTexture);
            }

            if (_useTemporaryBaseTextures) {
                ApplyTemporaryTextures();
            }
            LoadDetailedTextures();
        }

        protected override void GenerateMesh() {
            TerrainModelMeshMetadata metadata = GenerateMeshMetadata();
            UVBounds uvBounds = BoundingBoxUtils.CalculateUVBounds(SquareBoundingBox, BoundingBox);
            GenerateTerrainMeshTask generateBaseMeshTask = new GenerateBaseLocalTerrainMeshTask(metadata, BoundingBox, uvBounds);

            // Generate a base mesh first to be displayed temporarily
            // while the DEM data is being loaded.
            generateBaseMeshTask.Execute(meshData => {
                _referenceMeshData = meshData;
                QueueTask(() => {
                    ProcessMeshData(meshData);
                    PostProcessPlaceholderMeshData(meshData);
                    _initTaskStatus = TaskStatus.Completed;
                });
            });

            TerrainModelProductMetadata demMetadata = 
                new TerrainModelProductMetadata(DemUUID, SquareBoundingBox, LocalTerrainDemTargetSize, ImageFileFormat.Tiff);

            // Load the DEM data, and then generate another mesh after using the data.
            RasterSubsetWebService.SubsetProduct(demMetadata, filepath => {
                GenerateTerrainMeshTask generateMeshTask = 
                    new GenerateLocalTerrainMeshFromDigitalElevationModeTask(filepath, metadata, BoundingBox, uvBounds);

                // Generate the high detailed mesh using the DEM.
                _generateDetailedMeshTaskStatus = TaskStatus.InProgress; // TODO Move this before DEM retraval?
                generateMeshTask.Execute(meshData => {
                    _referenceMeshData = meshData;
                    QueueTask(() => {
                        if (_lodGroupContainer) {
                            Destroy(_lodGroupContainer);
                        }
                        ProcessMeshData(meshData);
                        PostProcessDetailedMeshData(meshData, metadata);
                        _generateDetailedMeshTaskStatus = TaskStatus.Completed;
                    });
                });
            });

        }

        protected override bool CanRescaleTerrainHeight() {
            return isActiveAndEnabled
                && _generateDetailedMeshTaskStatus == TaskStatus.Completed 
                && _heightRescaleTaskStatus != TaskStatus.InProgress;
        }

        protected override void RescaleTerrainHeight(float scale) {
            TerrainModelMeshMetadata metadata = GenerateMeshMetadata();
            RescaleTerrainMeshHeightTask rescaleMeshHeightTask = new RescaleLocalTerrainMeshHeightTask(_referenceMeshData, metadata);
            _heightRescaleTaskStatus = TaskStatus.InProgress;
            rescaleMeshHeightTask.Execute(rescaledMeshData => {
                QueueTask(() => {
                    ApplyRescaledMeshData(rescaledMeshData);

                    // Update the physics mesh if applicable.
                    int physicsMeshIndex = metadata.PhyiscsLodMeshIndex;
                    MeshCollider collider = GetComponent<MeshCollider>();
                    if (physicsMeshIndex >= 0 && collider) {
                        UpdateMesh(collider.sharedMesh, rescaledMeshData[physicsMeshIndex], false);
                    }

                    // Use the last LOD to recalculate position since it contains the least vertices.
                    RepositionModel(rescaledMeshData[LodLevels]);

                    _heightRescaleTaskStatus = TaskStatus.Completed;
                });
            });
        }

        /// <summary>
        ///     Applies the global textures to the mesh as a placeholder until
        ///     the more detailed local textures can be loaded.
        /// </summary>
        private void ApplyTemporaryTextures() {
            TerrainModelTextureManager textureManager = TerrainModelTextureManager.Instance;

            textureManager.GetTexture(BaseMosaicProduct, texture => {
                int diffuseBaseId = Shader.PropertyToID("_DiffuseBase");
                Material.SetTexture(diffuseBaseId, texture);

                UVScaleOffset uvScaleOffset = BoundingBoxUtils.CalculateUVScaleOffset(BaseMosaicProduct.BoundingBox, SquareBoundingBox);
                Material.SetTextureScale(diffuseBaseId, uvScaleOffset.Scale);
                Material.SetTextureOffset(diffuseBaseId, uvScaleOffset.Offset);
            });

            // TODO Inherit layers from globe terrain model.
        }

        /// <summary>
        ///     Loads the local textures and replaces the placeholder textures.
        /// </summary>
        private void LoadDetailedTextures() {
            TerrainModelTextureManager textureManager = TerrainModelTextureManager.Instance;

            // Store this locally for now so that the original struct can be accessed.
            TerrainModelProductMetadata baseMosaicProduct = GenerateProductMetadata(BaseMosaicProduct.ProductUUID);

            // Load and apply base texture
            textureManager.GetTexture(baseMosaicProduct, texture => {
                int diffuseBaseId = Shader.PropertyToID("_DiffuseBase");
                Material.SetTexture(diffuseBaseId, texture);
                Material.SetTextureScale(diffuseBaseId, Vector2.one);
                Material.SetTextureOffset(diffuseBaseId, Vector2.zero);
                textureManager.RegisterUsage(baseMosaicProduct, true);

                // Update the base mosaic product info.
                _baseMosaicProduct = baseMosaicProduct;
            });

            // TODO Inherit layers from globe terrain model.
        }

        /// <summary>
        ///     Positions the model after the basic placeholder mesh is generated,
        ///     and starts the view transition process.
        /// </summary>
        private void PostProcessPlaceholderMeshData(MeshData[] meshData) {

            GameObject meshContainer = _lodGroupContainer.transform.GetChild(0).gameObject;

            // Copy the placeholder mesh to use as a shadow caster.
            MeshFilter meshFilter = meshContainer.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;
            AddShadowCaster(mesh);

            // Calculate the mesh boundaries.
            float meshOffset = CalculateMeshOffset(meshData[0]);
            Bounds bounds = mesh.bounds;
            float meshWidth = Mathf.Max(bounds.size.y, bounds.size.z);

            // Compute the target transformations.
            _targetScale = TableWidth / meshWidth;
            _targetPosition = (_targetScale * -meshOffset + TableHeight + TableTopGap) * Vector3.up;
            _targetRotation = Quaternion.Euler(0, 0, 90);

            if (AnimateOnInitialization) {

                // Initially position the mesh to match its visual position on the globe.
                Vector2 latLongOffset = BoundingBoxUtils.MedianLatLon(BoundingBox);
                Quaternion rotation = TerrainModelManager.Instance.GlobeModel.transform.rotation;
                rotation *= Quaternion.Euler(0, -latLongOffset.y - 90, 0);
                rotation *= Quaternion.Euler(0, 0, latLongOffset.x);
                transform.rotation = rotation;
                transform.Translate(Radius * TerrainModelScale, 0, 0);

                _startPosition = transform.position;
                _startRotation = transform.rotation;
                _viewTransitionTaskStatus = TaskStatus.InProgress;
            }
            else {

                // Skip directly to final transform.
                transform.rotation = _targetRotation;
                transform.position = _targetPosition;
                transform.localScale = _targetScale * Vector3.one;
                _viewTransitionTaskStatus = TaskStatus.Completed;
            }

        }

        private void PostProcessDetailedMeshData(MeshData[] meshData, TerrainModelMeshMetadata metadata) {

            // Add mesh collider, if a physics mesh was generated.
            int physicsMeshIndex = metadata.PhyiscsLodMeshIndex;
            if (physicsMeshIndex < 0) {
                return;
            }
            // Do the heavy processing in the next update (does this help with the stutter?).
            QueueTask(() => {
                MeshCollider collider = gameObject.AddComponent<MeshCollider>();
                Mesh physicsMesh = ConvertToMesh(meshData[physicsMeshIndex], false);
                collider.sharedMesh = physicsMesh;
            });

            // Use the last LOD to recalculate position since it contains the least vertices.
            RepositionModel(meshData[LodLevels]);
        }

        private void RepositionModel(MeshData referenceMeshData) {
            float meshOffset = CalculateMeshOffset(referenceMeshData);
            Vector3 newPosition = (_targetScale * -meshOffset + TableHeight + TableTopGap) * Vector3.up;
            if (_viewTransitionTaskStatus == TaskStatus.InProgress) {
                _targetPosition = newPosition;
            }
            else {
                transform.position = newPosition;
            }
        }

        private void TransitionView() {
            _viewTransitionProgress += Time.deltaTime / ViewTransitionDuration;

            transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, _viewTransitionProgress * 2);
            transform.position = Vector3.Lerp(_startPosition, _targetPosition, _viewTransitionProgress);
            transform.localScale = Vector3.Lerp(Vector3.one, _targetScale * Vector3.one, _viewTransitionProgress);

            // TODO Animate tabletop...

            if (_viewTransitionProgress >= 1.0f) {
                _viewTransitionTaskStatus = TaskStatus.Completed;
            }
        }

        private float CalculateMeshOffset(MeshData meshData) {
            if (meshData.Vertices == null) {
                return default;
            }
            float min = float.MaxValue;
            foreach (Vector3 vertex in meshData.Vertices) {
                if (vertex.x < min) {
                    min = vertex.x;
                }
            }
            return min;
        }

        private TerrainModelProductMetadata GenerateProductMetadata(string productId, int size = LocalTerrainTextureTargetSize) {
            return new TerrainModelProductMetadata(productId, SquareBoundingBox, size);
        }

    }

}