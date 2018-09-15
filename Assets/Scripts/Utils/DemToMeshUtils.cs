using System;
using System.IO;
using UnityEngine;
using BitMiracle.LibTiff.Classic;

public class DemToMeshUtils {

    // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.

    /// <summary>
    ///     Generates a 3D mesh using the provided TIFF data elevation model (DEM) file and the provided parameters. 
    /// </summary>
    /// <param name="filePath">
    ///     Path to the TIFF DEM file. File extension can be anything as long as the file is in TIFF format.
    /// </param>
    /// <returns>
    ///     A Mesh object that can be attached to a MeshFilter.
    /// </returns>
    public static Mesh GenerateMesh(String filePath, TerrainGeometryType surfaceType, float scale, float heightScale, int downsample = 1) {

        // TODO Check if mesh is null

        if (String.IsNullOrEmpty(filePath)) {
            throw new FileNotSpecifiedException("No TIFF file specified for " + typeof(DemToMeshUtils).Name);
        }

        // TODO Maybe support other image types?

        using (Tiff tiff = Tiff.Open(filePath, "r")) {

            if (tiff == null) {
                throw new FileReadException("Cannot open TIFF from " + filePath);
            }

            return GenerateMesh(tiff, surfaceType, scale, heightScale, downsample);
        }

    }

    /// <summary>
    ///     Generates a 3D mesh using the provided TIFF data elevation model (DEM) file and the provided parameters. 
    /// </summary>
    /// <param name="bytes">
    ///     The raw bytes of the TIFF DEM file.
    /// </param>
    /// <returns>
    ///     A Mesh object that can be attached to a MeshFilter.
    /// </returns>
    public static Mesh GenerateMesh(byte[] bytes, TerrainGeometryType surfaceType, float scale, float heightScale, int downsample = 1) {

        using (Tiff tiff = Tiff.ClientOpen("in-memory", "r", new MemoryStream(bytes), new TiffStream())) {

            if (tiff == null) {
                throw new FileReadException("Cannot open TIFF from memory.");
            }

            return GenerateMesh(tiff, surfaceType, scale, heightScale, downsample);
        }

    }

    public static Mesh GenerateMesh(Tiff tiff, TerrainGeometryType surfaceType, float scale, float heightScale, int downsample = 1) {

        FieldValue[] res = tiff.GetField(TiffTag.BITSPERSAMPLE);
        short bpp = res[0].ToShort();

        res = tiff.GetField(TiffTag.SAMPLESPERPIXEL);
        short spp = res[0].ToShort();

        // Currently, only 16-bit and 32-bit grayscale files are supported.
        if (bpp != 32 && bpp != 16 || spp != 1) {
            throw new FileFormatException("Invalid TIFF format. Only 16-bit and 32-bit grayscale files are supported.");
        }

        res = tiff.GetField(TiffTag.IMAGELENGTH);
        int height = res[0].ToInt();

        res = tiff.GetField(TiffTag.IMAGEWIDTH);
        int width = res[0].ToInt();

        Debug.Log(width + "x" + height + "@" + bpp);

        Mesh mesh = new Mesh();

        // Set the index format of the mesh to 32-bits, so that the mesh can have more than 65k vertices.
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // TODO Check if the TIFF file is tiled instead of stripped by calling tiff.isTiled().
        // If it is tiled, then use ReadEncodedTile instead of ReadScanline.

        // Init the byte array for holding the data read from each TIFF scanline.
        byte[] scanline = new byte[tiff.ScanlineSize()];

        // Generate planar terrain mesh.
        if (surfaceType == TerrainGeometryType.Planar) {

            // Vertex counts in the horizontal and vertical directions are the
            // same as the downsampled texture width and height, respectively.
            int hVertCount = width / downsample;
            int vVertCount = height / downsample;

            Vector3[] vertices = new Vector3[hVertCount * vVertCount];
            Vector2[] uvs = new Vector2[hVertCount * vVertCount];

            // Vertex counter
            int vertexIndex = 0;

            // Lowest height value
            float min = float.MaxValue;

            for (int y = 0; y < vVertCount; y++) {
                tiff.ReadScanline(scanline, y * downsample);
                float[] values = bpp == 32 ? Scanline32ToFloat(scanline) : Scanline16ToFloat(scanline);
                for (int x = 0; x < hVertCount; x++) {
                    float value = values[x * downsample];
                    vertices[vertexIndex] = new Vector3(x * scale, value * heightScale, y * scale);
                    uvs[vertexIndex] = GenerateStandardUV(x, y, hVertCount, vVertCount);
                    min = value < min ? value : min;
                    vertexIndex++;
                }
            }

            // TODO Find a way to output the min value so that the mesh can be positioned correctly.
            // transform.position -= min * heightScale * Vector3.up;

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = GenerateTriangles(hVertCount, vVertCount);

            mesh.RecalculateNormals();

        }

        // Generate spherical terrain mesh.
        else if (surfaceType == TerrainGeometryType.Spherical) {

            // Vertex count for the latitude is the same as the downsampled texture height.
            // However, we need to generate an extra set of vertices in the longitude
            // direction to complete the loop around. We cannot simply reuse the first
            // vertices of of the loop, due to the start and end having different UV
            // coordinates despite having same world coordinates.
            int lonVertCount = width / downsample + 1;
            int latVertCount = height / downsample;

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
                float[] values = bpp == 32 ? Scanline32ToFloat(scanline) : Scanline16ToFloat(scanline);

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

        }

        return mesh;
    }

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
