using BitMiracle.LibTiff.Classic;
using UnityEngine;

public class TiffSphericalTerrainMeshGenerator : TiffTerrainMeshGenerator {

    private float _radius;

    public TiffSphericalTerrainMeshGenerator(string filepath, float radius, float heightScale, int lodLevels, int baseDownsample) : 
        base(filepath, heightScale, lodLevels, baseDownsample) {

        _radius = radius;
    }

    protected override MeshData Generate(Image<float> image, int downsample = 1) {

        // TODO Check if image dimensions and downsample rate are powers of 2.

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
                float scaled = value * _heightScale;

                // Longitude is offset by 90 degrees so that the foward vector is at 0,0 lat and long.
                verts[vertexIndex] = Quaternion.Euler(0, -90 - vx * lonStepSize, 0) * ((_radius + _heightScale * value) * baseLatVertex);
                uvs[vertexIndex] = GenerateStandardUV(vx, vy, lonVertCount, latVertCount);
                vertexIndex++;
            }
        }

        return new MeshData() {
            Vertices = verts,
            TexCoords = uvs,
            Triangles = GenerateTriangles(lonVertCount, latVertCount)
        };
    }

    protected override MeshData GenerateForScanlines(Tiff tiffImage, TiffMetadata info, int downsample) {

        TiffUtils.PrintInfo(tiffImage);

        // Init the byte array for holding the data read from each TIFF scanline.
        byte[] scanline = new byte[tiffImage.ScanlineSize()];

        // Init the float array for holding the data for each row.
        float[] rowValues = new float[info.Width];

        // Vertex count for the latitude is the same as the downsampled texture height.
        // However, we need to generate an extra set of vertices in the longitude
        // direction to complete the loop around. We cannot simply reuse the first
        // vertices of of the loop, due to the start and end having different UV
        // coordinates despite having same world coordinates.
        int lonVertCount = info.Width / downsample + 1;
        int latVertCount = info.Height / downsample;

        Debug.Log(lonVertCount + ", " + latVertCount);

        Vector3[] verts = new Vector3[lonVertCount * latVertCount];
        Vector2[] uvs = new Vector2[lonVertCount * latVertCount];

        // Vertex counter
        int vertexIndex = 0;

        // Calculate the incretmental step sizes of the latitude
        // and longitude here for potential performance increase.
        float latStepSize = Mathf.PI / (latVertCount - 1);
        float lonStepSize = 360.0f / (lonVertCount - 1);

        for (int y = 0; y < latVertCount; y++) {

            tiffImage.ReadScanline(scanline, y * downsample);
            if (info.BPP == 32) {
                TiffUtils.SingleToFloat(scanline, rowValues);
            }
            else {
                TiffUtils.Int16ToFloat(scanline, rowValues);
            }

            // Calculate the actual angle of the latitude.
            float latAng = latStepSize * y + Mathf.PI / 2;

            // Create a new vertex using the latitude angle. The coordinates of this
            // vertex will serve as a base for all the other vertices in this latitude.
            Vector3 baseLatVertex = new Vector3(Mathf.Cos(latAng), Mathf.Sin(latAng), 0);

            // Loop traverses backwards in order to get correct orientation of texture and normals.
            for (int x = lonVertCount - 1; x >= 0; x--) {
                float value = rowValues[Mathf.Clamp(x * downsample, 0, rowValues.Length - 1)];

                // Longitude is offset by 90 degrees so that the foward vector is at 0,0 lat and long.
                verts[vertexIndex] = Quaternion.Euler(0, -90 - x * lonStepSize, 0) * ((_radius + _heightScale * value) * baseLatVertex);
                uvs[vertexIndex] = GenerateStandardUV(x, y, lonVertCount, latVertCount);
                vertexIndex++;
            }
        }

        return new MeshData() {
            Vertices = verts,
            TexCoords = uvs,
            Triangles = GenerateTriangles(lonVertCount, latVertCount)
        };

    }

    protected override MeshData GenerateForTiles(Tiff tiffImage, TiffMetadata info, int downsample) {
        // TODO Implement this
        throw new System.NotImplementedException();
    }

}