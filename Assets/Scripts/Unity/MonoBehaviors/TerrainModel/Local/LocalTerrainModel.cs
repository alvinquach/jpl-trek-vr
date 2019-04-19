using UnityEngine;
using static TrekVRApplication.BoundingBoxUtils;
using static TrekVRApplication.ServiceManager;
using static TrekVRApplication.TerrainConstants;

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
                    SquareBoundingBox = ExpandToSquare(value);
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

        private void OnDestroy() {
            // TODO Unregister all layers
            TerrainProductMetadata texInfo = GenerateProductMetadata(GlobalMosaicUUID);
            TerrainModelTextureManager.Instance.RegisterUsage(texInfo, false);
        }

        #endregion

        protected override void AddRenderTextureOverlay() {
            TerrainOverlayController overlayController = LocalTerrainOverlayController.Instance;
            if (overlayController) {
                LayerController.Material.SetTexture("_Overlay", overlayController.RenderTexture);
            }
        }

        protected override void GenerateMesh() {
            TerrainModelMeshMetadata metadata = GenerateMeshMetadata();
            UVBounds uvBounds = CalculateUVBounds(SquareBoundingBox, BoundingBox);
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

            // Then, start downloading the DEM data for generating higher detailed mesh.
            _generateDetailedMeshTaskStatus = TaskStatus.InProgress; 

            TerrainProductMetadata demMetadata = 
                new TerrainProductMetadata(DemUUID, SquareBoundingBox, LocalTerrainDemTargetSize, ImageFileFormat.Tiff);

            // If the bounding box crosses the +/- 180° longitude line, then the
            // DEM data will need to be retrieved in two parts and then merged.
            if (IsLongitudeWrapped(demMetadata.BoundingBox)) {

                string filepath1 = null;
                string filepath2 = null;

                // Get the first half of the DEM data.
                RasterSubsetWebService.SubsetProduct(UnwrapBoundingBoxLeft(demMetadata), filepath => {
                    filepath1 = filepath;

                    // If the second DEM was already finished downloading,
                    // then the DEMs can be processed.
                    if (filepath2 != null) {
                        ProcessDemFiles(new string[] { filepath1, filepath2 }, metadata, uvBounds);
                    }
                });

                // Get the second half of the DEM data.
                RasterSubsetWebService.SubsetProduct(UnwrapBoundingBoxRight(demMetadata), filepath => {
                    filepath2 = filepath;

                    // If the first DEM was already finished downloading,
                    // then the DEMs can be processed.
                    if (filepath1 != null) {
                        ProcessDemFiles(new string[] { filepath1, filepath2 }, metadata, uvBounds);
                    }
                });

            }

            // Else, the DEM file can be retrieved and processed as a whole.
            else {
                RasterSubsetWebService.SubsetProduct(demMetadata, filepath => {
                    ProcessDemFiles(new string[] { filepath }, metadata, uvBounds);
                });
            }

        }

        protected override bool CanRescaleTerrainHeight() {
            return isActiveAndEnabled
                && _generateDetailedMeshTaskStatus == TaskStatus.Completed 
                && _heightRescaleTaskStatus != TaskStatus.InProgress;
        }

        protected override void RescaleTerrainHeight(float scale) {
            TerrainModelMeshMetadata metadata = GenerateMeshMetadata();
            RescaleTerrainMeshHeightTask rescaleMeshHeightTask = 
                new RescaleLocalTerrainMeshHeightTask(_referenceMeshData, metadata);
            _heightRescaleTaskStatus = TaskStatus.InProgress;
            rescaleMeshHeightTask.Execute(rescaledMeshData => {
                QueueTask(() => {
                    ApplyRescaledMeshData(rescaledMeshData);

                    // Update the physics mesh if applicable.
                    int physicsMeshIndex = metadata.PhyiscsLodMeshIndex;
                    if (physicsMeshIndex >= 0) {
                        _physicsMeshUpdateTimer = PhysicsMeshUpdateDelay;
                        _physicsMeshUpdatedData = rescaledMeshData[physicsMeshIndex];
                    }

                    // Use the last LOD to recalculate position since it contains the least vertices.
                    RepositionModel(rescaledMeshData[LodLevels]);

                    _heightRescaleTaskStatus = TaskStatus.Completed;
                });
            });
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
                Vector2 latLongOffset = MedianLatLon(BoundingBox);
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

        private void ProcessDemFiles(string[] filepaths, TerrainModelMeshMetadata metadata, UVBounds uvBounds) {
            GenerateTerrainMeshTask generateMeshTask = 
                new GenerateLocalTerrainMeshFromDigitalElevationModelTask(filepaths, metadata, BoundingBox, uvBounds);

            // Generate the high detailed mesh using the DEM.
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

        private TerrainProductMetadata GenerateProductMetadata(string productId, int size = LocalTerrainTextureTargetSize) {
            return new TerrainProductMetadata(productId, SquareBoundingBox, size);
        }

        private TerrainProductMetadata UnwrapBoundingBoxLeft(TerrainProductMetadata metadata) {
            return new TerrainProductMetadata(
                metadata.ProductUUID,
                UnwrapLeft(metadata.BoundingBox),
                metadata.Width,
                metadata.Height
            );
        }

        private TerrainProductMetadata UnwrapBoundingBoxRight(TerrainProductMetadata texInfo) {
            return new TerrainProductMetadata(
                texInfo.ProductUUID,
                UnwrapRight(texInfo.BoundingBox),
                texInfo.Width,
                texInfo.Height
            );
        }

    }

}