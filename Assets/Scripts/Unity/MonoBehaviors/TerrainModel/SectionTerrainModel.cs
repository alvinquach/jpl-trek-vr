using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    [RequireComponent(typeof(XRInteractableTerrainSection))]
    public class SectionTerrainModel : TerrainModel {

        private const float ViewTransitionDuration = 1.6f;

        private IDigitalElevationModelWebService _dataElevationModelWebService = TrekDigitalElevationModelWebService.Instance;

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
            if (_viewTransitionTaskStatus == TaskStatus.Started) {
                TransitionView();
            }
        }

        protected override void OnDestroy() {
            // TODO Unregister all layers
            TerrainModelProductMetadata texInfo = GenerateTerrainModelProductMetadata(GlobalMosaicUUID);
            TerrainModelTextureManager.Instance.RegisterUsage(texInfo, false);
        }

        #endregion

        protected override void GenerateMaterial() {
            base.GenerateMaterial();
            ApplyTemporaryTextures();
            LoadDetailedTextures();
        }

        protected override void GenerateMesh() {
            TerrainModelMeshMetadata metadata = GenerateTerrainModelMeshMetadata();
            UVBounds uvBounds = BoundingBoxUtils.CalculateUVBounds(SquareBoundingBox, BoundingBox);
            GenerateTerrainMeshTask generateBaseMeshTask = new GenerateBaseSectionTerrainMeshTask(metadata, BoundingBox, uvBounds);

            // Generate a base mesh first to be displayed temporarily
            // while the DEM data is being loaded.
            generateBaseMeshTask.Execute(meshData => {
                QueueTask(() => {
                    ProcessMeshData(meshData);
                    PostProcessPlaceholderMeshData();
                    _initTaskStatus = TaskStatus.Completed;
                });
            });

            // Load the DEM data, and then generate another mesh after using the data.
            _dataElevationModelWebService.GetDEM(SquareBoundingBox, 1024, demFilePath => {
                //_demFilePath = demFilePath; // Should this be allowed?
                metadata.DemFilePath = demFilePath; // Temporary fix
                GenerateTerrainMeshTask generateMeshTask = 
                    new GenerateSectionTerrainMeshFromDigitalElevationModeTask(metadata, BoundingBox, uvBounds);

                // Generate the high detailed mesh using the DEM.
                generateMeshTask.Execute((meshData) => {
                    QueueTask(() => {
                        if (_lodGroupContainer) {
                            Destroy(_lodGroupContainer);
                        }
                        ProcessMeshData(meshData);
                        PostProcessDetailedMeshData(meshData, metadata);
                    });
                });
            });

        }

        /// <summary>
        ///     Applies the global textures to the mesh as a placeholder until
        ///     the more detailed local textures can be loaded.
        /// </summary>
        private void ApplyTemporaryTextures() {
            // TODO Inherit layers from globe terrain model.
            TerrainModelTextureManager.Instance.GetGlobalMosaicTexture(texture => {
                int diffuseBaseId = Shader.PropertyToID("_DiffuseBase");
                Material.SetTexture(diffuseBaseId, texture);

                UVScaleOffset uvScaleOffset = BoundingBoxUtils.CalculateUVScaleOffset(UnrestrictedBoundingBox.Global, SquareBoundingBox);
                Material.SetTextureScale(diffuseBaseId, uvScaleOffset.Scale);
                Material.SetTextureOffset(diffuseBaseId, uvScaleOffset.Offset);
            });
        }

        /// <summary>
        ///     Loads the local textures and replaces the placeholder textures.
        /// </summary>
        private void LoadDetailedTextures() {
            // TODO Inherit layers from globe terrain model.

            TerrainModelTextureManager textureManager = TerrainModelTextureManager.Instance;
            TerrainModelProductMetadata texInfo = GenerateTerrainModelProductMetadata(GlobalMosaicUUID);
            textureManager.GetTexture(texInfo, texture => {
                int diffuseBaseId = Shader.PropertyToID("_DiffuseBase");
                Material.SetTexture(diffuseBaseId, texture);
                Material.SetTextureScale(diffuseBaseId, Vector2.one);
                Material.SetTextureOffset(diffuseBaseId, Vector2.zero);
                textureManager.RegisterUsage(texInfo, true);
            });

        }

        /// <summary>
        ///     Positions the model after the basic placeholder mesh is generated,
        ///     and starts the view transition process.
        /// </summary>
        private void PostProcessPlaceholderMeshData() {

            GameObject meshContainer = _lodGroupContainer.transform.GetChild(0).gameObject;

            // Copy the placeholder mesh to use as a shadow caster.
            MeshFilter meshFilter = meshContainer.GetComponent<MeshFilter>();
            AddShadowCaster(meshFilter.mesh);

            // Calculate the dimensions of the mesh in world space.
            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();
            Bounds bounds = meshRenderer.bounds;
            float meshDepth = bounds.size.x;
            float meshWidth = Mathf.Max(bounds.size.y, bounds.size.z);
            VectorUtils.Print(bounds.size);

            // Initially position the mesh to match its visual position on the globe.
            Vector2 latLongOffset = BoundingBoxUtils.MedianLatLon(BoundingBox);
            Quaternion rotation = TerrainModelManager.Instance.GetGlobeModelTransform().rotation;
            rotation *= Quaternion.Euler(0, -latLongOffset.y - 90, 0);
            rotation *= Quaternion.Euler(0, 0, latLongOffset.x);
            transform.rotation = rotation;
            transform.Translate(Radius * TerrainModelScale, 0, 0);

            // TODO Move these to constants file.
            float tableWidth = 2.0f;
            float tableHeight = 0.5f;
            float tableTopGap = 0.05f;

            // Start the view transition.
            _targetScale = tableWidth / meshWidth;
            _startPosition = transform.position;
            _targetPosition = (_targetScale * meshDepth + tableHeight + tableTopGap) * Vector3.up;
            _startRotation = transform.rotation;
            _targetRotation = Quaternion.Euler(0, 0, 90);
            _viewTransitionTaskStatus = TaskStatus.Started;
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
                Mesh physicsMesh = ConvertToMesh(meshData[physicsMeshIndex]);
                collider.sharedMesh = physicsMesh;
            });
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

        private TerrainModelProductMetadata GenerateTerrainModelProductMetadata(string productId, int size = 1024) {
            return new TerrainModelProductMetadata(productId, SquareBoundingBox, size);
        }

    }

}