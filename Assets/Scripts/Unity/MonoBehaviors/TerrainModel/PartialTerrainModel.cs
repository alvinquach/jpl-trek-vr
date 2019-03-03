using UnityEngine;

namespace TrekVRApplication {

    public class PartialTerrainModel : TerrainModel {

        private IDigitalElevationModelWebService _dataElevationModelWebService = TrekDigitalElevationModelWebService.Instance;

        private IMosaicWebService _mosaicWebService = TrekMosaicWebService.Instance;

        [SerializeField]
        protected float _heightScale = 1.0f;
        public override float HeightScale {
            get { return _heightScale; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _heightScale = value; }
        }

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


        /// <summary>
        ///     For generating a mesh without any height data. The gernated mesh will be
        ///     temporarily displayed while the DEM and textures are retrieved, and a new
        ///     mesh with height data is generated.
        /// </summary>
        private GenerateBasePartialTerrainMeshTask _baseMeshGenerator;

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

        public override void InitModel() {
            if (_initTaskStatus > TaskStatus.NotStarted) {
                return;
            }

            _initTaskStatus = TaskStatus.Started;

            TerrainModelMetadata metadata = GenerateMetadata();
            UVBounds uvBounds = BoundingBoxUtils.CalculateUVBounds(_squareBoundingBox, _boundingBox);
            GenerateTerrainMeshTask generateBaseMeshTask = new GenerateBasePartialTerrainMeshTask(metadata, _boundingBox, uvBounds);

            // Generate a base mesh first to be displayed temporarily
            // while the DEM data is being loaded.
            generateBaseMeshTask.Execute((meshData) => {
                QueueTask(() => ProcessMeshData(meshData));
                _initTaskStatus = TaskStatus.Completed;
            });

            // Load the DEM data, and then generate another mesh after using the data.
            _dataElevationModelWebService.GetDEM(_squareBoundingBox, 1024, (demFilePath) => {
                _demFilePath = demFilePath; // Should this be allowed?
                GenerateTerrainMeshTask generateMeshTask = InstantiateGenerateMeshTask();
                generateMeshTask.Execute((meshData) => {
                    QueueTask(() => {
                        // TODO Rework the logic in the base class to do this instead.
                        GameObject lodGroupContainer = transform.Find(GameObjectName.LODGroupContainer).gameObject;
                        if (lodGroupContainer) {
                            Destroy(lodGroupContainer);
                        }
                        base.ProcessMeshData(meshData);
                    });
                });
            });

            _mosaicWebService.GetMosaic(_squareBoundingBox, 1024, (textureFilePath) => {
                _albedoFilePath = textureFilePath; // Should this be allowed?

                // TODO Restructure base class to remove this duplicate logic.
                TextureCompressionFormat textureFormat = TextureCompressionFormat.DXT1;
                ConvertTextureFromFileTask textureTask = new ConvertTextureFromFileTask(textureFilePath, textureFormat);
                textureTask.Execute((data) => {
                    int width = textureTask.TextureWidth, height = textureTask.TextureHeight;
                    QueueTask(() => {
                        Texture2D texture = new Texture2D(width, height, textureFormat.GetUnityFormat(), true);
                        texture.GetRawTextureData<byte>().CopyFrom(data);
                        texture.Apply();
                        CurrentMaterial.SetTexture("_MainTex", texture); // Assume Material is not null or default.
                    });

                });
            });

        }

        protected override void ProcessMeshData(MeshData[] meshData) {
            base.ProcessMeshData(meshData);
            transform.rotation = TerrainModelManager.Instance.GetGlobalPlanetModelTransform().rotation;
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

        protected override GenerateTerrainMeshTask InstantiateGenerateMeshTask() {
            TerrainModelMetadata metadata = GenerateMetadata();
            UVBounds uvBounds = BoundingBoxUtils.CalculateUVBounds(_squareBoundingBox, _boundingBox);
            return new GenerateDigitalElevationModelPartialTerrainMeshTask(metadata, _boundingBox, uvBounds);
        }

        protected override TerrainModelMetadata GenerateMetadata() {
            return new TerrainModelMetadata() {
                demFilePath = _demFilePath,
                albedoFilePath = _albedoFilePath,
                radius = _radius,
                heightScale = _heightScale,
                lodLevels = _lodLevels,
                baseDownsample = _baseDownsampleLevel
            };
        }

    }

}