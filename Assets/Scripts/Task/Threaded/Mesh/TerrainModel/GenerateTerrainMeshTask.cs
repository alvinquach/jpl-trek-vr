using UnityEngine;

namespace TrekVRApplication {

    public abstract class GenerateTerrainMeshTask : GenerateMeshTask {

        protected TerrainModelMetadata _metadata;

        public GenerateTerrainMeshTask(TerrainModelMetadata metadata) {
            _metadata = metadata;
            // TODO Check if baseDownsample is power of 2.
        }

    }

}
