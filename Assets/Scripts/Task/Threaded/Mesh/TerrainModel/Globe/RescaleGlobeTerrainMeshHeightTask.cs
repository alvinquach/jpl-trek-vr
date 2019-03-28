namespace TrekVRApplication {

    public class RescaleGlobeTerrainMeshHeightTask : RescaleTerrainMeshHeightTask {

        public RescaleGlobeTerrainMeshHeightTask(MeshData[] refernceMeshData, TerrainModelMeshMetadata metadata) : 
            base(refernceMeshData, metadata) {

        }

        protected override MeshData RescaleMeshHeight(MeshData referenceMeshData) {
            return new MeshData() {
                Vertices = referenceMeshData.Vertices
                // TODO Normals
            };
        }

    }

}
