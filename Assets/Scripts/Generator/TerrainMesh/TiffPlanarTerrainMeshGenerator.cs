using BitMiracle.LibTiff.Classic;
using UnityEngine;

public class TiffPlanarTerrainMeshGenerator : TiffTerrainMeshGenerator {

    private float _size;

    public TiffPlanarTerrainMeshGenerator(string filepath, float size, float heightScale, int lodLevels, int baseDownsample) :
        base(filepath, heightScale, lodLevels, baseDownsample) {

        _size = size;
    }

    protected override MeshData GenerateForScanlines(Tiff tiffImage, TiffInfo info, int downsample) {

        // Init the byte array for holding the data read from each TIFF scanline.
        byte[] scanline = new byte[tiffImage.ScanlineSize()];

        // Vertex counts in the horizontal and vertical directions are the
        // same as the downsampled texture width and height, respectively.
        int hVertCount = info.Width / downsample;
        int vVertCount = info.Height / downsample;

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

        for (int y = 0; y < vVertCount; y++) {
            tiffImage.ReadScanline(scanline, y * downsample);
            float[] values = info.BPP == 32 ? TiffUtils.Scanline32ToFloat(scanline) : TiffUtils.Scanline16ToFloat(scanline);
            for (int x = 0; x < hVertCount; x++) {
                float value = values[x * downsample] * _heightScale;
                verts[vertexIndex] = new Vector3(x * dimScale - hOffset, value, y * dimScale - vOffset);
                uvs[vertexIndex] = GenerateStandardUV(x, y, hVertCount, vVertCount);
                min = value < min ? value : min;
                vertexIndex++;
            }
        }

        // TODO Is there a better way to do this?
        for (int i = 0; i < verts.Length; i++) {
            verts[i].y -= min;
        }

        int[] tris = GenerateTriangles(hVertCount, vVertCount);

        return new MeshData() {
            Vertices = verts,
            TexCoords = uvs,
            Triangles = tris
        };

    }

    protected override MeshData GenerateForTiles(Tiff tiffImage, TiffInfo info, int downsample) {
        throw new System.NotImplementedException();
    }

}