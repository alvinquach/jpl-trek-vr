using UnityEngine;

namespace TrekVRApplication {

    public class PlanarTerrainModel : TerrainModel {

        [SerializeField]
        private float _size;

        public float Size {
            get { return _size; }
            set { if (_initTaskStatus == TaskStatus.NotStarted) _size = value; }
        }

        protected override void ProcessMeshData(MeshData[] meshData) {
            base.ProcessMeshData(meshData);

            // TEMPORARY
            transform.localPosition = new Vector3(0, 0.31f, 0);
        }

        protected override GenerateTerrainMeshTask InstantiateGenerateMeshTask() {
            TerrainModelMetadata metadata = GenerateMetadata();
            return new GenerateDigitalElevationModelPlanarTerrainMeshTask(metadata);
        }

        protected override TerrainModelMetadata GenerateMetadata() {
            return new TerrainModelMetadata() {
                demFilePath = _demFilePath,
                albedoFilePath = _albedoFilePath,
                size = _size,
                heightScale = _heightScale,
                lodLevels = _lodLevels,
                baseDownsample = _baseDownsampleLevel
            };
        }

    }

}
