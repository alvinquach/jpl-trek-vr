using UnityEngine;
using static TrekVRApplication.SectionTerrainMeshGenerationUtils;

namespace TrekVRApplication {

    /// <summary>
    ///     Generates the mesh for a partial section of the planet defined
    ///     by bounding box coordinates. Does not use any height data;
    ///     as such, the resulting mesh will be a smooth surface.
    /// </summary>
    public class GenerateBaseSectionTerrainMeshTask : GenerateTerrainMeshTask {

        // TEMPORARY
        private static readonly int LatLongVertCount = 50;

        protected BoundingBox _boundingBox;
        protected UVBounds _uvBounds;

        public GenerateBaseSectionTerrainMeshTask(TerrainModelMetadata metadata,
            BoundingBox boundingBox, UVBounds uvBounds) : base(metadata) {

            _boundingBox = boundingBox;
            _uvBounds = uvBounds;
        }

        protected override void Generate() {

            float latIncrement = _boundingBox.LatSwing / (LatLongVertCount - 1);
            float lonIncrement = _boundingBox.LonSwing / (LatLongVertCount - 1);

            Vector3[] verts = new Vector3[LatLongVertCount * LatLongVertCount];
            Vector2[] uvs = new Vector2[LatLongVertCount * LatLongVertCount];

            Vector2 latLongOffset = BoundingBoxUtils.MedianLatLon(_boundingBox);

            int yIndex = 0, vertexIndex = 0;
            for (float vy = _boundingBox.LatStart; yIndex < LatLongVertCount; vy += latIncrement) {

                // Create a new vertex using the latitude angle. The coordinates of this vertex
                // will serve as a base for all the other vertices of the same latitude.
                Vector3 baseLatVertex = _metadata.Radius * GenerateBaseLatitudeVertex(vy);

                int xIndex = 0;
                for (float vx = _boundingBox.LonStart; xIndex < LatLongVertCount; vx += lonIncrement) {

                    verts[vertexIndex] = GenerateVertex(baseLatVertex, vx, latLongOffset, _metadata.Radius);
                    uvs[vertexIndex] = GenerateUVCoord(xIndex, yIndex, LatLongVertCount, LatLongVertCount, _uvBounds);

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
 