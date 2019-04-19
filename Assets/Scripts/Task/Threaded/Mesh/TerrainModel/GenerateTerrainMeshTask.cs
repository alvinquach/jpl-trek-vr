using UnityEngine;

namespace TrekVRApplication {

    public abstract class GenerateTerrainMeshTask : GenerateMeshTask<TerrainMeshData> {

        protected TerrainModelMeshMetadata _metadata;

        public GenerateTerrainMeshTask(TerrainModelMeshMetadata metadata) {
            _metadata = metadata;
            // TODO Check if baseDownsample is power of 2.
        }

    }

}
