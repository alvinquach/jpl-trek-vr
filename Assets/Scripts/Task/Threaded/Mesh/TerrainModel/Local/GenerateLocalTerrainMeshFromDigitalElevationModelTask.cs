using System;
using UnityEngine;
using static TrekVRApplication.LocalTerrainMeshGenerationUtils;
using static TrekVRApplication.MeshGenerationUtils;

namespace TrekVRApplication {

    /// <summary>
    ///     Generates the mesh for a localized section of the planet defined
    ///     by bounding box coordinates. Uses TIFF DEM files as a source of
    ///     height data.
    /// </summary>
    public class GenerateLocalTerrainMeshFromDigitalElevationModelTask : GenerateTerrainMeshFromDigitalElevationModelTask {

        protected BoundingBox _boundingBox;
        protected UVBounds _uvBounds;

        public GenerateLocalTerrainMeshFromDigitalElevationModelTask(string[] demFilePaths, TerrainModelMeshMetadata metadata,
            BoundingBox boundingBox, UVBounds uvBounds) : base(demFilePaths, metadata) {

            _boundingBox = boundingBox;
            _uvBounds = uvBounds;
        }

        protected override TerrainMeshData GenerateForLod(IntensityImage image, int downsample) {

            // Downsampling rate must be a power of 2.
            if (!MathUtils.IsPowerOfTwo(downsample)) {
                throw new Exception($"Downsample rate of {downsample} is not a power of 2.");
            }

            // Calculate image bounds based on UV bounds.
            int imageStartX = Mathf.RoundToInt(_uvBounds.U1 * image.Width / downsample);
            int imageEndX = Mathf.RoundToInt(_uvBounds.U2 * (image.Width / downsample - 1));
            int imageStartY = Mathf.RoundToInt(_uvBounds.V1 * image.Height / downsample);
            int imageEndY = Mathf.RoundToInt(_uvBounds.V2 * (image.Height / downsample - 1));

            // Each pixel in the selected area of the downsampled image represents a vertex.
            int lonVertCount = imageEndX - imageStartX + 1;
            int latVertCount = imageEndY - imageStartY + 1;

            float latIncrement = _boundingBox.LatSwing / (latVertCount - 1);
            float lonIncrement = _boundingBox.LonSwing / (lonVertCount - 1);

            Vector3[] verts = new Vector3[latVertCount * lonVertCount];
            Vector2[] uvs = new Vector2[latVertCount * lonVertCount];
            Vector3[] edgeVerts = new Vector3[4 * (latVertCount + lonVertCount - 2) + 2];

            Vector2 latLongOffset = BoundingBoxUtils.MedianLatLon(_boundingBox);

            Vector3 min = new Vector3(float.PositiveInfinity, 0, 0);

            int yIndex = 0, vertexIndex = 0;
            for (float vy = _boundingBox.LatStart; yIndex < latVertCount; vy += latIncrement) {

                // The y-coordinate on the image that corresponds to the current row of vertices.
                // Note this is actually inverted since we are traversing from bottom up.
                int y = (latVertCount - yIndex - 1 + imageStartY) * downsample;

                // Create a new vertex using the latitude angle. The coordinates of this vertex
                // will serve as a base for all the other vertices of the same latitude.
                Vector3 baseLatVertex = GenerateBaseLatitudeVertex(vy);

                int xIndex = 0;
                for (float vx = _boundingBox.LonStart; xIndex < lonVertCount; vx += lonIncrement) {

                    // The x-coordinate on the image that corresponds to the current vertex.
                    int x = (xIndex + imageStartX) * downsample;

                    // Get the raw intensity value from the image.
                    float value = downsample == 1 ?
                        image.GetPixel(x, y) :
                        image.GetCenteredAverage(x, y, downsample + 1);

                    // Scale the intensity value by the height scale, and
                    // then add it to the radius to get the final "height".
                    float height = value * _metadata.HeightScale + _metadata.Radius;

                    Vector3 vertex = GenerateVertex(height * baseLatVertex, vx, latLongOffset, _metadata.Radius);

                    // Keep track of minimum; this will be used later to position the terrain on the table-top.
                    if (vertex.x < min.x) {
                        min = vertex;
                    }

                    // Add to edge vertices
                    if (yIndex == 0) {
                        edgeVerts[xIndex] = vertex;
                    }
                    else if (xIndex == lonVertCount - 1) {
                        edgeVerts[lonVertCount + yIndex - 1] = vertex;
                    }
                    else if (yIndex == latVertCount - 1) {
                        edgeVerts[2 * (lonVertCount - 1) + latVertCount - xIndex - 1] = vertex;
                    }
                    else if (xIndex == 0) {
                        edgeVerts[2 * (lonVertCount + latVertCount - 2) - yIndex] = vertex;
                    }

                    verts[vertexIndex] = vertex;
                    uvs[vertexIndex] = GenerateUVCoord(xIndex, yIndex, lonVertCount, latVertCount, _uvBounds);

                    xIndex++;
                    vertexIndex++;
                }

                yIndex++;
            }

            // Finish generating the data for the terrain edge.
            ProcessEdgeVertices(edgeVerts, min.x);

            return new TerrainMeshData() {
                Vertices = verts,
                TexCoords = uvs,
                Triangles = GenerateTriangles(lonVertCount, latVertCount),
                ExtraVertices = edgeVerts,
                ExtraTriangles = GenerateTriangles(edgeVerts.Length / 2, 2, true),
                MinimumVertex = min
            };
        }

    }

}
