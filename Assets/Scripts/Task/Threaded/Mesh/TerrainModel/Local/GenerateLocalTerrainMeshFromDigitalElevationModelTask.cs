using System;
using UnityEngine;
using static TrekVRApplication.LocalTerrainMeshGenerationUtils;

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

        protected override MeshData GenerateForLod(IntensityImage image, int downsample) {

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

            Vector2 latLongOffset = BoundingBoxUtils.MedianLatLon(_boundingBox);

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

                    verts[vertexIndex] = GenerateVertex(height * baseLatVertex, vx, latLongOffset, _metadata.Radius);
                    uvs[vertexIndex] = GenerateUVCoord(xIndex, yIndex, lonVertCount, latVertCount, _uvBounds);

                    xIndex++;
                    vertexIndex++;
                }

                yIndex++;
            }

            return new MeshData() {
                Vertices = verts,
                TexCoords = uvs,
                Triangles = MeshGenerationUtils.GenerateTriangles(lonVertCount, latVertCount)
            };
        }

    }

}
