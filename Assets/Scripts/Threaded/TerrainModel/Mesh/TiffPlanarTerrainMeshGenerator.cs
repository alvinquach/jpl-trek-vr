using BitMiracle.LibTiff.Classic;
using UnityEngine;

public class TiffPlanarTerrainMeshGenerator : TiffTerrainMeshGenerator {

    private float _size;

    public TiffPlanarTerrainMeshGenerator(string filepath, float size, float heightScale, int lodLevels, int baseDownsample) :
        base(filepath, heightScale, lodLevels, baseDownsample) {

        _size = size;
    }

    protected override MeshData Generate(Image<float> image, int downsample = 1) {

        // TODO Check if image dimensions and downsample rate are powers of 2.

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

    protected override MeshData GenerateForScanlines(Tiff tiffImage, TiffMetadata info, int downsample) {

        // Init the byte array for holding the data read from each TIFF scanline.
        byte[] scanline = new byte[tiffImage.ScanlineSize()];

        // Init the float array for holding the data for each row.
        float[] rowValues = new float[info.Width];

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
            if (info.BPP == 32) {
                TiffUtils.SingleToFloat(scanline, rowValues);
            }
            else {
                TiffUtils.Int16ToFloat(scanline, rowValues);
            }

            for (int x = 0; x < hVertCount; x++) {
                float value = rowValues[x * downsample] * _heightScale;
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
    protected override MeshData GenerateForTiles(Tiff tiffImage, TiffMetadata info, int downsample) {

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

            // An array containing the valuse for a section or block of the entire iamge.
            float[,] imageBlock = new float[tileWidth * tilesAcrossImage, tileHeight];

            byte[] tileBytes = new byte[tileSize];
            for (int tx = 0; tx < info.Width; tx += tileWidth) {
                tiffImage.ReadTile(tileBytes, 0, tx, ty, 0, 0);
                TiffSampleFormat format = info.BPP == 32 ? TiffSampleFormat.SinglePrecisionFloat : TiffSampleFormat.SignedShort;
                TiffUtils.IntensityTileToFloat(tileBytes, format, imageBlock, tileWidth, new Vector2Int(tx, 0));
            }

            for (int y = ty / downsample; y < (ty + tileHeight) / downsample; y++) {

                if (vertexIndex >= verts.Length) {
                    break;
                }

                for (int x = 0; x < hVertCount; x++) {
                    float value = imageBlock[x * downsample, y * downsample - ty] * _heightScale;
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

}