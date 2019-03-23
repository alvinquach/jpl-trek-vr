using System;
using UnityEngine;

namespace TrekVRApplication {

    public class GenerateGlobeTerrainMeshFromDigitalElevationModelTask : GenerateTerrainMeshFromDigitalElevationModelTask {

        public GenerateGlobeTerrainMeshFromDigitalElevationModelTask(TerrainModelMetadata metadata) : base(metadata) {

        }

        protected override MeshData GenerateForLod(IntensityImage image, int downsample = 1) {

            // Downsampling rate must be a power of 2.
            if (!MathUtils.IsPowerOfTwo(downsample)) {
                throw new Exception($"Downsample rate of {downsample} is not a power of 2.");
            }

            // Vertex count for the latitude is the same as the downsampled texture height.
            // However, we need to generate an extra set of vertices in the longitude
            // direction to complete the loop around. We cannot simply reuse the first
            // vertices of of the loop, due to the start and end having different UV
            // coordinates despite having same world coordinates.
            int lonVertCount = image.Width / downsample + 1;
            int latVertCount = image.Height / downsample;

            Debug.Log(lonVertCount + ", " + latVertCount);

            Vector3[] verts = new Vector3[lonVertCount * latVertCount];
            Vector2[] uvs = new Vector2[lonVertCount * latVertCount];

            // Vertex counter
            int vertexIndex = 0;

            // Calculate the incretmental step sizes of the latitude
            // and longitude here for potential performance increase.
            float latStepSize = Mathf.PI / (latVertCount - 1);
            float lonStepSize = 360.0f / (lonVertCount - 1);

            // Iterate through the rows of vertices.
            for (int vy = 0; vy < latVertCount; vy++) {

                // The y-coordinate on the image that corresponds to the current row of vertices.
                int y = vy * downsample;

                // Iterate through each vertex in the row of verticies.
                // Calculate the actual angle of the latitude.
                float latAng = latStepSize * vy + Mathf.PI / 2;

                // Create a new vertex using the latitude angle. The coordinates of this
                // vertex will serve as a base for all the other vertices in this latitude.
                Vector3 baseLatVertex = new Vector3(Mathf.Cos(latAng), Mathf.Sin(latAng), 0);

                // Iterate through each vertex in the row of verticies.
                // Traverse backwards in order to get correct orientation of texture and normals.
                for (int vx = lonVertCount - 1; vx >= 0; vx--) {

                    // The x-coordinate on the image that corresponds to the current vertex.
                    int x = vx * downsample;

                    // Get the raw intensity value from the image.
                    float value = downsample == 1 ?
                        image.GetPixel(x, y) :
                        image.GetCenteredAverage(x, y, downsample + 1);

                    // Scale the intensity value by the height scale.
                    float scaled = value * _metadata.heightScale;

                    // Longitude is offset by 90 degrees so that the foward vector is at 0,0 lat and long.
                    verts[vertexIndex] = Quaternion.Euler(0, -90 - vx * lonStepSize, 0) * ((_metadata.radius + _metadata.heightScale * value) * baseLatVertex);
                    uvs[vertexIndex] = MeshGenerationUtils.GenerateUVCoord(vx, vy, lonVertCount, latVertCount);
                    vertexIndex++;
                }
            }

            return new MeshData() {
                Vertices = verts,
                TexCoords = uvs,
                Triangles = MeshGenerationUtils.GenerateTriangles(lonVertCount, latVertCount)
            };
        }

    }

}