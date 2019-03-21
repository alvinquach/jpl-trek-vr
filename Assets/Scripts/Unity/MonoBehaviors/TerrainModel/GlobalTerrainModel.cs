using UnityEngine;

namespace TrekVRApplication {

    [RequireComponent(typeof(XRInteractablePlanet))]
    public class GlobalTerrainModel : TerrainModel {

        public const float GlobalModelScale = 2.5e-7f;

        private float _radius;
        public float Radius {
            get { return _radius * GlobalModelScale; }
            set {
                if (_initTaskStatus == TaskStatus.NotStarted) {
                    _radius = value;
                }
            }
        }

        protected override void GenerateMaterials() {
            base.GenerateMaterials();

            TerrainModelTextureManager textureManager = TerrainModelTextureManager.Instance;
            textureManager.GetGlobalMosaicTexture(texture => {
                CurrentMaterial.SetTexture("_DiffuseBase", texture); // Assume Material is not null or default.
            });
        }

        protected override void GenerateMesh() {
            TerrainModelMetadata metadata = GenerateTerrainModelMetadata();
            GenerateTerrainMeshTask generateMeshTask = new GenerateDigitalElevationModelSphericalTerrainMeshTask(metadata);
            generateMeshTask.Execute((meshData) => {
                QueueTask(() => ProcessMeshData(meshData));
                _initTaskStatus = TaskStatus.Completed;
            });
        }

        protected override void ProcessMeshData(MeshData[] meshData) {
            base.ProcessMeshData(meshData);

            // Adds a sphere collider to the mesh, so that it can be manipulated using the controller.
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = Radius;
        }

        protected override TerrainModelMetadata GenerateTerrainModelMetadata() {
            return new TerrainModelMetadata() {
                demFilePath = _demFilePath,
                radius = Radius,
                heightScale = HeightScale * GlobalModelScale,
                lodLevels = _lodLevels,
                baseDownsample = _baseDownsampleLevel
            };
        }

        private TerrainModelProductMetadata GenerateTerrainModelProductMetadata(string productId, int width, int height) {
            return new TerrainModelProductMetadata(productId, BoundingBox.Global, width, height);
        }

    }

}
