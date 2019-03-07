using System;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Generates the mesh for a partial section of the planet defined
    ///     by bounding box coordinates. Uses TIFF DEM files as a source of
    ///     height data.
    /// </summary>
    public class GenerateDigitalElevationModelPartialTerrainMeshTask : GenerateDigitalElevationModelTerrainMeshTask {

        protected BoundingBox _boundingBox;
        protected UVBounds _uvBounds;

        public GenerateDigitalElevationModelPartialTerrainMeshTask(TerrainModelMetadata metadata,
            BoundingBox boundingBox, UVBounds uvBounds) : base(metadata) {

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

            float latStart = _boundingBox[1] * Mathf.Deg2Rad;
            float latStop = _boundingBox[3] * Mathf.Deg2Rad;
            float latSweep = latStop - latStart;

            bool reverseLonOrder = BoundingBoxUtils.ReverseLonOrder(_boundingBox);
            float lonStart = _boundingBox[reverseLonOrder ? 2 : 0];
            float lonStop = _boundingBox[reverseLonOrder ? 0 : 2];
            float lonSweep = Mathf.DeltaAngle(lonStart, lonStop);

            Debug.Log($"{latSweep * Mathf.Rad2Deg}, {lonSweep}");

            float latIncrement = latSweep / (latVertCount - 1);
            float lonIncrement = lonSweep / (lonVertCount - 1);

            Vector3[] verts = new Vector3[latVertCount * lonVertCount];
            Vector2[] uvs = new Vector2[latVertCount * lonVertCount];

            Vector3 offset = BoundingBoxUtils.MedianDirection(_boundingBox);

            int yIndex = 0, vertexIndex = 0;
            for (float vy = latStart; yIndex < latVertCount; vy += latIncrement) {

                // The y-coordinate on the image that corresponds to the current row of vertices.
                // Note this is actually inverted since we are traversing from bottom up.
                int y = (latVertCount - yIndex - 1 + imageStartY) * downsample;

                // Create a new vertex using the latitude angle. The coordinates of this
                // vertex will serve as a base for all the other vertices in this latitude.
                Vector3 baseLatVertex = new Vector3(Mathf.Cos(vy), Mathf.Sin(vy), 0);

                int xIndex = 0;
                for (float vx = lonStart; xIndex < lonVertCount; vx += lonIncrement) {

                    // The x-coordinate on the image that corresponds to the current vertex.
                    int x = (xIndex + imageStartX) * downsample;

                    // Get the raw intensity value from the image.
                    float value = downsample == 1 ?
                        image.GetPixel(x, y) :
                        image.GetCenteredAverage(x, y, downsample + 1);

                    // Scale the intensity value by the height scale.
                    float scaled = 1 + value * _metadata.heightScale / _metadata.radius;

                    // Longitude is offset by 90 degrees so that the foward vector is at 0,0 lat and long.
                    verts[vertexIndex] = _metadata.radius * (Quaternion.Euler(0, -90 - vx, 0) * (scaled * baseLatVertex) - offset);

                    Vector2 uvScale = new Vector2(_uvBounds.U2 - _uvBounds.U1, _uvBounds.V2 - _uvBounds.V1);
                    Vector2 uvOffset = new Vector2(-_uvBounds.U1, -_uvBounds.V1);
                    uvs[vertexIndex] = MeshGenerationUtils.GenerateUVCoord(xIndex, latVertCount - yIndex,
                        lonVertCount, latVertCount,uvScale, uvOffset);

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
