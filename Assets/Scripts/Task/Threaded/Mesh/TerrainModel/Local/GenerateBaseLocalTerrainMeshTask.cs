using UnityEngine;
using static TrekVRApplication.LocalTerrainMeshGenerationUtils;
using static TrekVRApplication.MeshGenerationUtils;

namespace TrekVRApplication {

    /// <summary>
    ///     Generates the mesh for a localized section of the planet defined
    ///     by bounding box coordinates. Does not use any height data;
    ///     as such, the resulting mesh will be a smooth surface.
    /// </summary>
    public class GenerateBaseLocalTerrainMeshTask : GenerateTerrainMeshTask {

        // TEMPORARY
        private static readonly int LatLongVertCount = 50;

        protected BoundingBox _boundingBox;
        protected UVBounds _uvBounds;

        public GenerateBaseLocalTerrainMeshTask(TerrainModelMeshMetadata metadata,
            BoundingBox boundingBox, UVBounds uvBounds) : base(metadata) {

            _boundingBox = boundingBox;
            _uvBounds = uvBounds;
        }

        protected override void Generate() {

            float latIncrement = _boundingBox.LatSwing / (LatLongVertCount - 1);
            float lonIncrement = _boundingBox.LonSwing / (LatLongVertCount - 1);

            Vector3[] verts = new Vector3[LatLongVertCount * LatLongVertCount];
            Vector2[] uvs = new Vector2[LatLongVertCount * LatLongVertCount];
            Vector3[] edgeVerts = new Vector3[8 * (LatLongVertCount - 1) + 2];

            Vector2 latLongOffset = BoundingBoxUtils.MedianLatLon(_boundingBox);

            Vector3 min = new Vector3(float.PositiveInfinity, 0, 0);

            int yIndex = 0, vertIndex = 0;
            for (float vy = _boundingBox.LatStart; yIndex < LatLongVertCount; vy += latIncrement) {

                // Create a new vertex using the latitude angle. The coordinates of this vertex
                // will serve as a base for all the other vertices of the same latitude.
                Vector3 baseLatVertex = _metadata.Radius * GenerateBaseLatitudeVertex(vy);

                int xIndex = 0;
                for (float vx = _boundingBox.LonStart; xIndex < LatLongVertCount; vx += lonIncrement) {
                    Vector3 vertex = GenerateVertex(baseLatVertex, vx, latLongOffset, _metadata.Radius);

                    // Keep track of minimum; this will be used later to position the terrain on the table-top.
                    if (vertex.x < min.x) {
                        min = vertex;
                    }

                    // Add to edge vertices
                    if (yIndex == 0) {
                        edgeVerts[xIndex] = vertex;
                    }
                    else if (xIndex == LatLongVertCount - 1) {
                        edgeVerts[LatLongVertCount + yIndex - 1] = vertex;
                    }
                    else if (yIndex == LatLongVertCount - 1) {
                        edgeVerts[3 * (LatLongVertCount - 1) - xIndex] = vertex;
                    }
                    else if (xIndex == 0) {
                        edgeVerts[4 * (LatLongVertCount - 1) - yIndex] = vertex;
                    }

                    verts[vertIndex] = vertex;
                    uvs[vertIndex] = GenerateUVCoord(xIndex, yIndex, LatLongVertCount, LatLongVertCount, _uvBounds);

                    xIndex++;
                    vertIndex++;
                }

                yIndex++;
            }

            // Finish generating the data for the terrain edge.
            ProcessEdgeVertices(edgeVerts, min.x);

            _progress = 1f;

            // Only one LOD is generated.
            _meshData = new TerrainMeshData[] {
                new TerrainMeshData() {
                    Vertices = verts,
                    TexCoords = uvs,
                    Triangles = GenerateTriangles(LatLongVertCount, LatLongVertCount),
                    ExtraVertices = edgeVerts,
                    ExtraTriangles = GenerateTriangles(edgeVerts.Length / 2, 2, true),
                    MinimumVertex = min
                }
            };

        }

    }

}
 