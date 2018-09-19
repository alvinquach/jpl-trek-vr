using System;
using System.IO;
using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class DemToMeshUtils {

    // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.
    // TODO Maybe support other image types?

    public static Mesh GeneratePlanarMesh(Tiff tiff, float size, float heightScale, int downsample = 4) {

        // Extract info from TIFF data.
        TiffInfo info = TiffUtils.GetInfo(tiff);

        // Check if the TIFF data is valid for mesh generation.
        // Exceptions are thrown if the TIFF is not valid.
        ValidateTiff(info);

        Mesh mesh = CreateNewMesh();

        // TODO Check if the TIFF file is tiled instead of stripped by calling tiff.isTiled().
        // If it is tiled, then use ReadEncodedTile instead of ReadScanline.

        // Init the byte array for holding the data read from each TIFF scanline.
        byte[] scanline = new byte[tiff.ScanlineSize()];

        // Vertex counts in the horizontal and vertical directions are the
        // same as the downsampled texture width and height, respectively.
        int hVertCount = info.Width / downsample;
        int vVertCount = info.Height / downsample;

        Vector3[] vertices = new Vector3[hVertCount * vVertCount];
        Vector2[] uvs = new Vector2[hVertCount * vVertCount];

        // For now, the scaling factor is based on the width of the DEM file.
        float dimScale = size / (hVertCount - 1);

        // Vertex counter
        int vertexIndex = 0;

        // Lowest height value
        float min = float.MaxValue;

        float hOffset = size / 2;
        float vOffset = dimScale * (vVertCount - 1) / 2;

        for (int y = 0; y < vVertCount; y++) {
            tiff.ReadScanline(scanline, y * downsample);
            float[] values = info.BPP == 32 ? Scanline32ToFloat(scanline) : Scanline16ToFloat(scanline);
            for (int x = 0; x < hVertCount; x++) {
                float value = values[x * downsample] * heightScale;
                vertices[vertexIndex] = new Vector3(x * dimScale - hOffset, value, y * dimScale - vOffset);
                uvs[vertexIndex] = GenerateStandardUV(x, y, hVertCount, vVertCount);
                min = value < min ? value : min;
                vertexIndex++;
            }
        }

        // TODO Is there a better way to do this?
        for (int i = 0; i < vertices.Length; i++) {

            vertices[i].y -= min;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = GenerateTriangles(hVertCount, vVertCount);

        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh GenerateSphericalMesh(Tiff tiff, float scale, float heightScale, int downsample = 1) {

        // Extract info from TIFF data.
        TiffInfo info = TiffUtils.GetInfo(tiff);

        // Check if the TIFF data is valid for mesh generation.
        // Exceptions are thrown if the TIFF is not valid.
        ValidateTiff(info);

        Mesh mesh = CreateNewMesh();

        // TODO Check if the TIFF file is tiled instead of stripped by calling tiff.isTiled().
        // If it is tiled, then use ReadEncodedTile instead of ReadScanline.

        // Init the byte array for holding the data read from each TIFF scanline.
        byte[] scanline = new byte[tiff.ScanlineSize()];

        // Vertex count for the latitude is the same as the downsampled texture height.
        // However, we need to generate an extra set of vertices in the longitude
        // direction to complete the loop around. We cannot simply reuse the first
        // vertices of of the loop, due to the start and end having different UV
        // coordinates despite having same world coordinates.
        int lonVertCount = info.Width / downsample + 1;
        int latVertCount = info.Height / downsample;

        Debug.Log(lonVertCount + ", " + latVertCount);

        Vector3[] vertices = new Vector3[lonVertCount * latVertCount];
        Vector2[] uvs = new Vector2[lonVertCount * latVertCount];

        // Vertex counter
        int vertexIndex = 0;

        // Calculate the incretmental step sizes of the latitude
        // and longitude here for potential performance increase.
        float latStepSize = Mathf.PI / (latVertCount - 1);
        float lonStepSize = 360.0f / (lonVertCount - 1);

        for (int y = 0; y < latVertCount; y++) {
            tiff.ReadScanline(scanline, y * downsample);
            float[] values = info.BPP == 32 ? Scanline32ToFloat(scanline) : Scanline16ToFloat(scanline);

            // Calculate the actual angle of the latitude.
            float latAng = latStepSize * y + Mathf.PI / 2;

            // Create a new vertex using the latitude angle. The coordinates of this
            // vertex will serve as a base for all the other vertices in this latitude.
            Vector3 baseLatVertex = new Vector3(Mathf.Cos(latAng), Mathf.Sin(latAng), 0);

            // Loop traverses backwards in order to get correct orientation of texture and normals.
            for (int x = lonVertCount - 1; x >= 0; x--) {
                float value = values[Mathf.Clamp(x * downsample, 0, values.Length - 1)];

                // Longitude is offset by 90 degrees so that the foward vector is at 0,0 lat and long.
                vertices[vertexIndex] = Quaternion.Euler(0, -90 - x * lonStepSize, 0) * ((scale + heightScale * value) * baseLatVertex);
                uvs[vertexIndex] = GenerateStandardUV(x, y, lonVertCount, latVertCount);
                vertexIndex++;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = GenerateTriangles(lonVertCount, latVertCount);

        Debug.Log(mesh.vertices.Length);

        mesh.RecalculateNormals();


        return mesh;
    }

    private static void ValidateTiff(TiffInfo info) {

        if (info == null) {
            throw new Exception("TIFF file cannot be null."); // TODO Create exception for this.
        }

        // Currently, only 16-bit and 32-bit grayscale files are supported.
        if (info.BPP != 32 && info.BPP != 16 || info.SPP != 1) {
            throw new FileFormatException("Invalid TIFF format. Only 16-bit and 32-bit grayscale files are supported.");
        }

    }

    private static Mesh CreateNewMesh() {
        Mesh mesh = new Mesh();

        // Set the index format of the mesh to 32-bits, so that the mesh can have more than 65k vertices.
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        return mesh;
    }

    #region Bit Conversion Methods

    private static float[] Scanline32ToFloat(byte[] scanline) {
        if (scanline.Length % 4 != 0) {
            return null;
        }
        int length = scanline.Length / 4;
        float[] result = new float[length];
        for (int i = 0; i < length; i++) {
            // TODO Check if CPU uses little endian.
            result[i] = BitConverter.ToSingle(scanline, 4 * i);
        }
        return result;
    }

    private static float[] Scanline16ToFloat(byte[] scanline) {
        if (scanline.Length % 2 != 0) {
            return null;
        }
        int length = scanline.Length / 2;
        float[] result = new float[length];
        for (int i = 0; i < length; i++) {
            // TODO Check if CPU uses little endian.
            result[i] = BitConverter.ToInt16(scanline, 2 * i);
        }
        return result;
    }

    #endregion

    private static int[] GenerateTriangles(int hVertCount, int vVertCount) {

        // The number of quads (triangle pairs) in each dimension is 
        // one less than the vertex counts in the respective dimensions.
        int hQuadCount = hVertCount - 1, vQuadCount = vVertCount - 1;

        int[] result = new int[6 * hQuadCount * vQuadCount];

        int startIndex = 0;
        for (int y = 0; y < vQuadCount; y++) {
            for (int x = 0; x < hQuadCount; x++) {

                int lt = x + y * hVertCount;
                int rt = lt + 1;
                int lb = lt + hVertCount;
                int rb = lb + 1;

                // TODO Alternate the triangle orientation of each consecutive quad.

                result[startIndex] = lt;
                result[startIndex + 1] = lb;
                result[startIndex + 2] = rb;
                result[startIndex + 3] = rb;
                result[startIndex + 4] = rt;
                result[startIndex + 5] = lt;

                startIndex += 6;
            }
        }
        return result;
    }

    private static Vector2 GenerateStandardUV(int x, int y, int width, int height) {
        return new Vector2(x / (width - 1f), -y / (height - 1f));
    }

}
