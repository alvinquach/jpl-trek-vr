namespace TrekVRApplication {

    public class RescaleSectionTerrainMeshHeightTask : RescaleTerrainMeshHeightTask {

        public RescaleSectionTerrainMeshHeightTask(MeshData[] refernceMeshData, TerrainModelMeshMetadata metadata) : 
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
