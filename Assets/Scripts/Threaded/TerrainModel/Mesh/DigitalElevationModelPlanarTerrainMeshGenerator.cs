using System;
using UnityEngine;

public class DigitalElevationModelPlanarTerrainMeshGenerator : DigitalElevationModelTerrainMeshGenerator {

    private float _size;

    public DigitalElevationModelPlanarTerrainMeshGenerator(string filepath, float size, float heightScale, int lodLevels, int baseDownsample) :
        base(filepath, heightScale, lodLevels, baseDownsample) {

        _size = size;
    }

    protected override MeshData Generate(Image<float> image, int downsample = 1) {

        // Downsampling rate must be a power of 2.
        if (!MathUtils.IsPowerOfTwo(downsample)) {
            throw new Exception($"Downsample rate of {downsample} is not a power of 2.");
        }

        // Vertex counts in the horizontal and vertical directions are the
        // same as the downsampled texture width and height, respectively.
        int hVertCount = image.Width / downsample;
        int vVertCount = image.Height / downsample;

        Vector3[] verts = new Vector3[hVertCount * vVertCount];
        Vector2[] uvs = new Vector2[hVertCount * vVertCount];

        // For now, the scaling factor is based on the width of the DEM file.
        float dimScale = _size / (hVertCount - 1);

        // Vertex counter
        int vertexIndex = 0;

        // Lowest height value
        float min = float.MaxValue;

        float hOffset = _size / 2;
        float vOffset = dimScale * (vVertCount - 1) / 2;

        // Iterate through the rows of vertices.
        for (int vy = 0; vy < vVertCount; vy++) {

            // The y-coordinate on the image that corresponds to the current row of vertices.
            int y = vy * downsample;

            // Iterate through each vertex in the row of verticies.
            for (int vx = 0; vx < hVertCount; vx++) {

                // The x-coordinate on the image that corresponds to the current vertex.
                int x = vx * downsample;

                // Get the raw intensity value from the image.
                float value = downsample == 1 ?
                    image.GetPixel(x, y) :
                    image.GetCenteredAverage(x, y, downsample + 1);
                //float value = image.GetPixel(x, y);

                // Scale the intensity value by the height scale.
                float scaled = value * _heightScale;

                verts[vertexIndex] = new Vector3(vx * dimScale - hOffset, scaled, vy * dimScale - vOffset);
                uvs[vertexIndex] = GenerateStandardUV(vx, vy, hVertCount, vVertCount);
                min = scaled < min ? scaled : min;
                vertexIndex++;
            }
        }

        // TODO Is there a better way to do this?
        for (int i = 0; i < verts.Length; i++) {
            verts[i].y -= min;
        }

        return new MeshData() {
            Vertices = verts,
            TexCoords = uvs,
            Triangles = GenerateTriangles(hVertCount, vVertCount)
        };
    }

}