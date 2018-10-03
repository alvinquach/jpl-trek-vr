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

        return new MeshData() {
            Vertices = verts,
            TexCoords = uvs,
            Triangles = GenerateTriangles(hVertCount, vVertCount)
        };

    }

    // TODO REDO THIS METHOD
    protected override MeshData GenerateForTiles(Tiff tiffImage, TiffInfo info, int downsample) {

        int tileSize = tiffImage.TileSize(); // bytes

        int tileWidth = tiffImage.GetField(TiffTag.TILEWIDTH)[0].ToInt();
        int tileHeight = tiffImage.GetField(TiffTag.TILELENGTH)[0].ToInt();

        int tilesAcrossImage = Mathf.CeilToInt(info.Width / (float)tileWidth);

        Debug.Log($"Tile Size: {tileWidth}x{tileHeight}, {tileSize} bytes");

        byte[] tileBuffer = new byte[tileSize * tilesAcrossImage];

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


        for (int ty = 0; ty < info.Height; ty += tileHeight) {
            float[,] blockValues = new float[tileHeight, tileWidth * tilesAcrossImage];
            for (int tx = 0; tx < info.Width; tx += tileWidth) {
                byte[] tile = new byte[tileSize];
                tiffImage.ReadTile(tile, 0, tx, ty, 0, 0);
                float[] tileValues = info.BPP == 32 ? TiffUtils.Scanline32ToFloat(tile) : TiffUtils.Scanline16ToFloat(tile);
                CopyValues(blockValues, tileValues, tileWidth, tileHeight, tx);
            }

            for (int y = ty / downsample; y < (ty + tileHeight) / downsample; y++) {

                if (vertexIndex >= verts.Length) {
                    break;
                }

                for (int x = 0; x < hVertCount; x++) {
                    float value = blockValues[y * downsample - ty, x * downsample] * _heightScale;
                    verts[vertexIndex] = new Vector3(x * dimScale - hOffset, value, y * dimScale - vOffset);
                    uvs[vertexIndex] = GenerateStandardUV(x, y, hVertCount, vVertCount);
                    min = value < min ? value : min;
                    vertexIndex++;
                }
            }
        }

        Debug.Log(vertexIndex);

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

    private void CopyValues(float[,] dest, float[] src, int tileWidth, int tileHeight, int offset) {

        if (src.Length != tileWidth * tileHeight) {
            throw new System.Exception("Incorrect data size");
        }

        int index = 0;
        for (int y = 0; y < tileHeight; y++) {
            for (int x = 0; x < tileWidth; x++) {
                dest[y, x + offset] = src[index++];
            }
        }

    }

}