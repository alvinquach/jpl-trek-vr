using BitMiracle.LibTiff.Classic;
using System;

public abstract class TiffTerrainMeshGenerator : TerrainMeshGenerator {

    protected int _baseDownsample;

    public TiffTerrainMeshGenerator(string filepath, float heightScale, int lodLevels, int baseDownsample) :
        base(filepath, heightScale, lodLevels) {

        _baseDownsample = baseDownsample;
    }

    // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.
    // TODO Maybe support other image types?

    public override void Generate() {

        using (Tiff tiffImage = TiffUtils.FromFilePath(_filepath)) {

            // Extract info from TIFF data.
            TiffInfo info = TiffUtils.GetInfo(tiffImage);

            // Check if the TIFF data is valid for mesh generation.
            // Exceptions are thrown if the TIFF is not valid.
            ValidateTiff(info);

            MeshData[] meshData = new MeshData[_lodLevels + 1];

            // Init the byte array for holding the data read from each TIFF scanline.
            // byte[] scanline = new byte[tiffImage.ScanlineSize()];

            for (int lodLevel = 0; lodLevel <= _lodLevels; lodLevel++) {
                int downsample = lodLevel + _baseDownsample;

                meshData[lodLevel] = info.Tiled?
                    GenerateForTiles(tiffImage, info, downsample):
                    GenerateForScanlines(tiffImage, info, downsample);
            }

            InProgress = false;
            Complete = true;
            Progress = 1f;
            MeshData = meshData;
        }

    }

    // TODO Modify the implementations of the functions below such
    // that they generate all the LODs at the same time, instead of
    // having to read the image data once for each LOD.

    protected abstract MeshData GenerateForScanlines(Tiff tiffImage, TiffInfo info, int downsample);

    protected abstract MeshData GenerateForTiles(Tiff tiffImage, TiffInfo info, int downsample);

    /// <summary>
    ///     Checks of the Tiff image can be used as a height map for generating meshes.
    ///     For use as a height map, the image must be in 16-bit or 32-bit grayscale.
    ///     Both scanline and tiled encoding are support.
    ///     Throws an error if an invalid or incompatible Tiff image is used.
    /// </summary>
    protected void ValidateTiff(TiffInfo info) {

        if (info == null) {
            throw new Exception("TIFF file cannot be null."); // TODO Create exception for this.
        }

        // Currently, only 16-bit and 32-bit grayscale files are supported.
        if (info.BPP != 32 && info.BPP != 16 || info.SPP != 1) {
            throw new FileFormatException("Invalid TIFF format. Only 16-bit and 32-bit grayscale files are supported.");
        }

    }

    protected int[] GenerateTriangles(int hVertCount, int vVertCount) {

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

}
