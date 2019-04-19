using UnityEngine;
using static TrekVRApplication.TerrainConstants;

namespace TrekVRApplication {

    public class RescaleGlobeTerrainMeshHeightTask : RescaleTerrainMeshHeightTask {

        public RescaleGlobeTerrainMeshHeightTask(TerrainMeshData[] refernceMeshData, TerrainModelMeshMetadata metadata) : 
            base(refernceMeshData, metadata) {

        }

        protected override TerrainMeshData RescaleMeshHeight(TerrainMeshData referenceMeshData) {

            float scale = _metadata.HeightScale / TerrainModelScale;
            float radius = _metadata.Radius;

            Vector3[] referenceVertices = referenceMeshData.Vertices;
            int vertexCount = referenceVertices.Length;
            Vector3[] rescaledVertices = new Vector3[vertexCount];

            for (int i = 0; i < vertexCount; i++) {
                Vector3 vertex = referenceVertices[i];

                // Calculate new distance
                float distance = vertex.magnitude;
                float newDistance = (distance - radius) * scale + radius;

                // Apply new distance
                vertex *= newDistance / distance;

                rescaledVertices[i] = vertex;
            }

            return new TerrainMeshData() {
                Vertices = rescaledVertices
                // TODO Normals
            };
        }

    }

}
