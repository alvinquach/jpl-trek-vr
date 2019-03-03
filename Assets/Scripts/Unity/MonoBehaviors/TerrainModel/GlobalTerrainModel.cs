using UnityEngine;

namespace TrekVRApplication {

    [RequireComponent(typeof(XRInteractablePlanet))]
    public class GlobalTerrainModel : TerrainModel {

        public const float GlobalModelScale = 2.5e-7f;

        public override float HeightScale {
            get { return GlobalModelScale; }
            set { }
        }

        [SerializeField]
        private float _radius;

        public float Radius {
            get { return _radius; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _radius = value * GlobalModelScale; }
        }

        protected override void ProcessMeshData(MeshData[] meshData) {
            base.ProcessMeshData(meshData);

            // Add a sphere collider to the mesh, so that it can be manipulated using the controller.
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = _radius;
        }

        protected override GenerateTerrainMeshTask InstantiateGenerateMeshTask() {
            TerrainModelMetadata metadata = GenerateMetadata();
            return new GenerateDigitalElevationModelSphericalTerrainMeshTask(metadata);
        }

        protected override TerrainModelMetadata GenerateMetadata() {
            return new TerrainModelMetadata() {
                demFilePath = _demFilePath,
                albedoFilePath = _albedoFilePath,
                radius = _radius,
                heightScale = HeightScale,
                lodLevels = _lodLevels,
                baseDownsample = _baseDownsampleLevel
            };
        }

        protected override void OnMaterialApplied(Material material) {
            TerrainModelManager.Instance.GlobalPlanetMaterial = material;
        }

        protected override void OnTextureApplied(Texture2D texture) {
            TerrainModelManager.Instance.GlobalPlanetTexture = texture;
        }

    }

}
