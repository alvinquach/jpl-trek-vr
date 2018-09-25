using BitMiracle.LibTiff.Classic;
using UnityEngine;

public class TiffSphericalTerrainMeshGenerator : TiffTerrainMeshGenerator {

    public TiffSphericalTerrainMeshGenerator(string filepath) : base(filepath) {

    }

    public override MeshData[] Generate(float radius, float heightScale, int lodLevels, int baseDownsample) {

        using (Tiff tiffImage = TiffUtils.FromFilePath(_filepath)) {

            // Extract info from TIFF data.
            TiffInfo info = TiffUtils.GetInfo(tiffImage);

            // Check if the TIFF data is valid for mesh generation.
            // Exceptions are thrown if the TIFF is not valid.
            ValidateTiff(info);

            MeshData[] meshData = new MeshData[lodLevels + 1];

            // TODO Check if the TIFF file is tiled instead of stripped by calling tiff.isTiled().
            // If it is tiled, then use ReadEncodedTile instead of ReadScanline.

            // Init the byte array for holding the data read from each TIFF scanline.
            byte[] scanline = new byte[tiffImage.ScanlineSize()];

            for (int lodLevel = 0; lodLevel <= lodLevels; lodLevel++) {

                int downsample = lodLevel + baseDownsample;

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
                    float[] values = info.BPP == 32 ? TiffUtils.Scanline32ToFloat(scanline) : TiffUtils.Scanline16ToFloat(scanline);

                    // Calculate the actual angle of the latitude.
                    float latAng = latStepSize * y + Mathf.PI / 2;

                    // Create a new vertex using the latitude angle. The coordinates of this
                    // vertex will serve as a base for all the other vertices in this latitude.
                    Vector3 baseLatVertex = new Vector3(Mathf.Cos(latAng), Mathf.Sin(latAng), 0);

                    // Loop traverses backwards in order to get correct orientation of texture and normals.
                    for (int x = lonVertCount - 1; x >= 0; x--) {
                        float value = values[Mathf.Clamp(x * downsample, 0, values.Length - 1)];

                        // Longitude is offset by 90 degrees so that the foward vector is at 0,0 lat and long.
                        verts[vertexIndex] = Quaternion.Euler(0, -90 - x * lonStepSize, 0) * ((radius + heightScale * value) * baseLatVertex);
                        uvs[vertexIndex] = GenerateStandardUV(x, y, lonVertCount, latVertCount);
                        vertexIndex++;
                    }
                }

                int[] tris = GenerateTriangles(lonVertCount, latVertCount);

                meshData[lodLevel] = new MeshData() {
                    Vertices = verts,
                    TexCoords = uvs,
                    Triangles = tris
                };

            }

            InProgress = false;
            Complete = true;
            Progress = 1f;
            return MeshData = meshData;
        }

    }


}