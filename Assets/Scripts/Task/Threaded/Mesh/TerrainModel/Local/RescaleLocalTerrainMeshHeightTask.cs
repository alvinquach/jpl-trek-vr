using UnityEngine;
using static TrekVRApplication.TerrainConstants;

namespace TrekVRApplication {

    public class RescaleLocalTerrainMeshHeightTask : RescaleTerrainMeshHeightTask {

        public RescaleLocalTerrainMeshHeightTask(TerrainMeshData[] refernceMeshData, TerrainModelMeshMetadata metadata) : 
            base(refernceMeshData, metadata) {

        }

        protected override TerrainMeshData RescaleMeshHeight(TerrainMeshData referenceMeshData) {

            float scale = _metadata.HeightScale / TerrainModelScale;
            float radius = _metadata.Radius;

            Vector3 min = new Vector3(float.PositiveInfinity, 0, 0);

            Vector3[] referenceVertices = referenceMeshData.Vertices;
            int vertexCount = referenceVertices.Length;
            Vector3[] rescaledVertices = new Vector3[vertexCount];

            for (int i = 0; i < vertexCount; i++) {

                // Undo radius offset
                Vector3 vertex = referenceVertices[i] + new Vector3(radius, 0, 0);

                // Calculate new distance
                float distance = vertex.magnitude;
                float newDistance = (distance - radius) * scale + radius;

                // Apply new distance
                vertex *= newDistance / distance;

                // Redo radius offset
                vertex -= new Vector3(radius, 0, 0);

                // Store vertex in new array.
                rescaledVertices[i] = vertex;

                // Keep track of minimum.
                if (vertex.x < min.x) {
                    min = vertex;
                }
            }

            Vector3[] referenceExtraVertices = referenceMeshData.ExtraVertices;
            int extraVertexCount = referenceExtraVertices.Length;
            Vector3[] rescaledExtraVertices = new Vector3[extraVertexCount];

            for (int i = 0; i < extraVertexCount; i++) {
                Vector3 vertex;

                // Only the first half of the vertex follows the terrain coutours;
                // the second half is the flat base.
                if (i >= extraVertexCount / 2) {
                    vertex = referenceExtraVertices[i];
                    rescaledExtraVertices[i] = new Vector3(min.x, vertex.y, vertex.z);
                    continue;
                }

                // Undo radius offset
                vertex = referenceExtraVertices[i] + new Vector3(radius, 0, 0);

                // Calculate new distance
                float distance = vertex.magnitude;
                float newDistance = (distance - radius) * scale + radius;

                // Apply new distance
                vertex *= newDistance / distance;

                // Redo radius offset
                vertex -= new Vector3(radius, 0, 0);

                // Store vertex in new array.
                rescaledExtraVertices[i] = vertex;
            }


            return new TerrainMeshData() {
                Vertices = rescaledVertices,
                ExtraVertices = rescaledExtraVertices,
                MinimumVertex = min
                // TODO Normals
            };
        }

    }

}
