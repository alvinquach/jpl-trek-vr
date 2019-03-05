﻿using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Generates the mesh for a partial section of the planet defined
    ///     by bounding box coordinates. Does not use any height data;
    ///     as such, the resulting mesh will be a smooth surface.
    /// </summary>
    public class GenerateBasePartialTerrainMeshTask : GenerateTerrainMeshTask {

        // TEMPORARY
        private static readonly int LatLongVertCount = 50;

        protected BoundingBox _boundingBox;
        protected UVBounds _uvBounds;

        public GenerateBasePartialTerrainMeshTask(TerrainModelMetadata metadata,
            BoundingBox boundingBox, UVBounds uvBounds) : base(metadata) {

            _boundingBox = boundingBox;
            _uvBounds = uvBounds;
        }

        protected override void Generate() {

            float latStart = _boundingBox[1] * Mathf.Deg2Rad;
            float latStop = _boundingBox[3] * Mathf.Deg2Rad;
            float latSweep = latStop - latStart;

            bool reverseLonOrder = BoundingBoxUtils.ReverseLonOrder(_boundingBox);
            float lonStart = _boundingBox[reverseLonOrder ? 2 : 0];
            float lonStop = _boundingBox[reverseLonOrder ? 0 : 2];
            float lonSweep = Mathf.DeltaAngle(lonStart, lonStop);

            Debug.Log($"{latSweep * Mathf.Rad2Deg}, {lonSweep}");

            float latIncrement = latSweep / (LatLongVertCount - 1);
            float lonIncrement = lonSweep / (LatLongVertCount - 1);

            Vector3[] verts = new Vector3[LatLongVertCount * LatLongVertCount];
            Vector2[] uvs = new Vector2[LatLongVertCount * LatLongVertCount];

            Vector3 offset = BoundingBoxUtils.MedianDirection(_boundingBox);

            int yIndex = 0, vertexIndex = 0;
            for (float vy = latStart; yIndex < LatLongVertCount; vy += latIncrement) {

                // Create a new vertex using the latitude angle. The coordinates of this
                // vertex will serve as a base for all the other vertices in this latitude.
                Vector3 baseLatVertex = new Vector3(Mathf.Cos(vy), Mathf.Sin(vy), 0);

                int xIndex = 0;
                for (float vx = lonStart; xIndex < LatLongVertCount; vx += lonIncrement) {

                    // Longitude is offset by 90 degrees so that the foward vector is at 0,0 lat and long.
                    verts[vertexIndex] = _metadata.radius * (Quaternion.Euler(0, -90 - vx, 0) * baseLatVertex - offset);

                    Vector2 uvScale = new Vector2(_uvBounds.U2 - _uvBounds.U1, _uvBounds.V2 - _uvBounds.V1);
                    Vector2 uvOffset = new Vector2(-_uvBounds.U1, -_uvBounds.V1);
                    uvs[vertexIndex] = MeshGenerationUtils.GenerateUVCoord(xIndex, LatLongVertCount - yIndex, LatLongVertCount,
                        LatLongVertCount, uvScale, uvOffset);

                    xIndex++;
                    vertexIndex++;
                }
                yIndex++;
            }

            _progress = 1f;

            // Only one LOD is generated.
            _meshData = new MeshData[] {
                new MeshData() {
                    Vertices = verts,
                    TexCoords = uvs,
                    Triangles = MeshGenerationUtils.GenerateTriangles(LatLongVertCount, LatLongVertCount)
                }
            };

        }

    }

}
 