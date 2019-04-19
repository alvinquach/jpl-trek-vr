namespace TrekVRApplication {

    public abstract class RescaleTerrainMeshHeightTask : ThreadedTask<float, TerrainMeshData[]> {

        protected TerrainModelMeshMetadata _metadata;

        protected TerrainMeshData[] _referenceMeshData;

        protected TerrainMeshData[] _rescaledMeshData;

        protected float _progress = 0.0f;
        public override float Progress => _progress;

        public RescaleTerrainMeshHeightTask(TerrainMeshData[] refernceMeshData, TerrainModelMeshMetadata metadata) {
            _referenceMeshData = refernceMeshData;
            _metadata = metadata;
        }

        protected sealed override TerrainMeshData[] Task() {
            int meshCount = _referenceMeshData.Length;
            TerrainMeshData[] rescaledMeshData = new TerrainMeshData[meshCount];
            for (int i = 0; i < meshCount; i++) {
                rescaledMeshData[i] = RescaleMeshHeight(_referenceMeshData[i]);
            }

            _progress = 1.0f;
            _rescaledMeshData = rescaledMeshData;
            return _rescaledMeshData;
        }

        /// <summary>Rescale the mesh data and store it in the member variable.</summary>
        protected abstract TerrainMeshData RescaleMeshHeight(TerrainMeshData referenceMeshData);

    }

}