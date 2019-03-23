using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    public class SectionTerrainModel : TerrainModel {

        private IDigitalElevationModelWebService _dataElevationModelWebService = TrekDigitalElevationModelWebService.Instance;

        [SerializeField]
        private float _radius;
        public float Radius {
            get { return _radius; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _radius = value; }
        }

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
        private Quaternion _startRotation;
        private Quaternion _targetRotation;
        private float _targetScale;

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

                generateMeshTask.Execute((meshData) => {
                    QueueTask(() => {
                        // TODO Rework the logic in the base class to do this instead.
                        GameObject lodGroupContainer = transform.Find(GameObjectName.LODGroupContainer).gameObject;
                        if (lodGroupContainer) {
                            Destroy(lodGroupContainer);
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
            transform.rotation = TerrainModelManager.Instance.GetGlobeModelTransform().rotation;
            transform.localPosition = transform.rotation * (0.25f * 3.39f * BoundingBoxUtils.MedianDirection(_boundingBox));

            // Start the view transition.
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _targetRotation = Quaternion.FromToRotation(BoundingBoxUtils.MedianDirection(_boundingBox), Vector3.up);
            _targetScale = 0.5f / BoundingBoxUtils.LargestDimension(_boundingBox);
            _viewTransitionTaskStatus = TaskStatus.Started;
        }

        private void TransitionView() {
            // TODO Un-hardcode these values
            _viewTransitionProgress += Time.deltaTime / 1.337f;
            transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, _viewTransitionProgress);
            transform.position = Vector3.Lerp(_startPosition, 0.69f * Vector3.up, _viewTransitionProgress);
            transform.localScale = Vector3.Lerp(0.25f * Vector3.one, _targetScale * Vector3.one, _viewTransitionProgress);
            if (_viewTransitionProgress >= 1.0f) {
                _viewTransitionTaskStatus = TaskStatus.Completed;
            }
        }

        protected override TerrainModelMetadata GenerateTerrainModelMetadata() {
            return new TerrainModelMetadata() {
                demFilePath = _demFilePath,
                radius = _radius,
                heightScale = _heightScale,
                lodLevels = _lodLevels,
                baseDownsample = _baseDownsampleLevel
            };
        }

        private TerrainModelProductMetadata GenerateTerrainModelProductMetadata(string productId, int size = 1024) {
            return new TerrainModelProductMetadata(productId, _squareBoundingBox, size);
        }

    }

}