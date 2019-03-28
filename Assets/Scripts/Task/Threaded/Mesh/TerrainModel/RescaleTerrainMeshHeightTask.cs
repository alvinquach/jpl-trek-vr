namespace TrekVRApplication {

    public abstract class RescaleTerrainMeshHeightTask : ThreadedTask<float, MeshData[]> {

        protected TerrainModelMeshMetadata _metadata;

        protected MeshData[] _referenceMeshData;

        protected MeshData[] _rescaledMeshData;

        protected float _progress = 0.0f;
        public override float Progress => _progress;

        public RescaleTerrainMeshHeightTask(MeshData[] refernceMeshData, TerrainModelMeshMetadata metadata) {
            _referenceMeshData = refernceMeshData;
            _metadata = metadata;
        }

        protected sealed override MeshData[] Task() {
            int meshCount = _referenceMeshData.Length;
            MeshData[] rescaledMeshData = new MeshData[meshCount];
            for (int i = 0; i < meshCount; i++) {
                rescaledMeshData[i] = RescaleMeshHeight(_referenceMeshData[i]);
            }

            _progress = 1.0f;
            _rescaledMeshData = rescaledMeshData;
            return _rescaledMeshData;
        }

        /// <summary>Rescale the mesh data and store it in the member variable.</summary>
        protected abstract MeshData RescaleMeshHeight(MeshData referenceMeshData);

    }

}