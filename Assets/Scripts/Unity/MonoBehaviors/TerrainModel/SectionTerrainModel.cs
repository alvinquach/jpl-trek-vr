using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    public class SectionTerrainModel : TerrainModel {

        private const float ViewTransitionDuration = 1.6f;

        private IDigitalElevationModelWebService _dataElevationModelWebService = TrekDigitalElevationModelWebService.Instance;

        private BoundingBox _boundingBox;
        private BoundingBox _squareBoundingBox;

        public BoundingBox BoundingBox {
            get { return _boundingBox; }
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    _boundingBox = value;
                    _squareBoundingBox = BoundingBoxUtils.ExpandToSquare(value);
                }
            }
        }

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
            TerrainModelMetadata metadata = GenerateTerrainModelMetadata();
            UVBounds uvBounds = BoundingBoxUtils.CalculateUVBounds(_squareBoundingBox, _boundingBox);
            GenerateTerrainMeshTask generateBaseMeshTask = new GenerateBaseSectionTerrainMeshTask(metadata, _boundingBox, uvBounds);

            // Generate a base mesh first to be displayed temporarily
            // while the DEM data is being loaded.
            generateBaseMeshTask.Execute((meshData) => {
                QueueTask(() => {
                    ProcessMeshData(meshData);
                    PostProcessMeshData();
                    _initTaskStatus = TaskStatus.Completed;
                });
            });

            // Load the DEM data, and then generate another mesh after using the data.
            _dataElevationModelWebService.GetDEM(_squareBoundingBox, 1024, demFilePath => {
                //_demFilePath = demFilePath; // Should this be allowed?
                metadata.demFilePath = demFilePath; // Temporary fix
                GenerateTerrainMeshTask generateMeshTask = 
                    new GenerateSectionTerrainMeshFromDigitalElevationModeTask(metadata, _boundingBox, uvBounds);

                // Generate the high detailed mesh using the DEM.
                generateMeshTask.Execute((meshData) => {
                    QueueTask(() => {
                        Transform lodGroupContainer = transform.Find(GameObjectName.LODGroupContainer);
                        if (lodGroupContainer) {
                            Destroy(lodGroupContainer.gameObject);
                        }
                        ProcessMeshData(meshData);
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

                UVScaleOffset uvScaleOffset = BoundingBoxUtils.CalculateUVScaleOffset(UnrestrictedBoundingBox.Global, _squareBoundingBox);
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
        ///     Positions the model after the mesh is generated, and starts the view transition process.
        /// </summary>
        private void PostProcessMeshData() {

            Transform lodGroupContainer = transform.Find(GameObjectName.LODGroupContainer);

            // Calculate the dimensions of the mesh in world space.
            GameObject meshContainer = lodGroupContainer.GetChild(0).gameObject;
            MeshRenderer meshRenderer = meshContainer.GetComponent<MeshRenderer>();
            Bounds bounds = meshRenderer.bounds;
            float meshDepth = bounds.size.x;
            float meshWidth = Mathf.Max(bounds.size.y, bounds.size.z);
            VectorUtils.Print(bounds.size);

            // Initially position the mesh to match its visual position on the globe.
            //lodGroupContainer.localPosition = new Vector3(Radius * TerrainModelScale, 0, 0);

            Vector2 latLongOffset = BoundingBoxUtils.MedianLatLon(_boundingBox);
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
            return new TerrainModelProductMetadata(productId, _squareBoundingBox, size);
        }

    }

}