﻿using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    public class RescaleSectionTerrainMeshHeightTask : RescaleTerrainMeshHeightTask {

        public RescaleSectionTerrainMeshHeightTask(MeshData[] refernceMeshData, TerrainModelMeshMetadata metadata) : 
            base(refernceMeshData, metadata) {

        }

        protected override MeshData RescaleMeshHeight(MeshData referenceMeshData) {

            float scale = _metadata.HeightScale / TerrainModelScale;
            float radius = _metadata.Radius;

            Vector3[] referenceVertices = referenceMeshData.Vertices;
            int vertexCount = referenceVertices.Length;
            Vector3[] rescaledVertices = new Vector3[vertexCount];

            for (int i = 0; i < vertexCount; i++) {
                Vector3 vertex;

                // Undo radius offset
                vertex = referenceVertices[i] + new Vector3(radius, 0, 0);

                // Calculate new distance
                float distance = vertex.magnitude;
                float newDistance = (distance - radius) * scale + radius;

                // Apply new distance
                vertex *= newDistance / distance;

                // Redo radius offset
                vertex -= new Vector3(radius, 0, 0);

                rescaledVertices[i] = vertex;
            }

            return new MeshData() {
                Vertices = rescaledVertices
                // TODO Normals
            };
        }

    }

}
