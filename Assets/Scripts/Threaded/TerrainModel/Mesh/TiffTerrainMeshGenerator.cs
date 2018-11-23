using BitMiracle.LibTiff.Classic;
using System;

public abstract class TiffTerrainMeshGenerator : TerrainMeshGenerator {

    protected string _filepath;

    protected int _lodLevels;

    protected int _baseDownsample;

    protected float _heightScale;

    public TiffTerrainMeshGenerator(string filepath, float heightScale, int lodLevels, int baseDownsample) {

        // TODO Check if baseDownsample is power of 2.

        _filepath = filepath;
        _lodLevels = lodLevels;
        _heightScale = heightScale;
        _baseDownsample = baseDownsample;
    }

    // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.
    // TODO Maybe support other image types?

    public override void Generate() {

        using (TiffWrapper tiffImage = new TiffWrapper(_filepath)) {

            // Check if the TIFF data is valid for mesh generation.
            // Exceptions are thrown if the TIFF is not valid.
            ValidateTiff(tiffImage.Metadata);

            MeshData[] meshData = new MeshData[_lodLevels + 1];

            Image<float> image = tiffImage.ToIntensityImage();

            for (int lodLevel = 0; lodLevel <= _lodLevels; lodLevel++) {
                int downsample = _baseDownsample << lodLevel;
                meshData[lodLevel] = Generate(image, downsample);
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

    protected abstract MeshData Generate(Image<float> image, int downsample);

    [Obsolete]
    protected abstract MeshData GenerateForScanlines(Tiff tiffImage, TiffMetadata info, int downsample);

    [Obsolete]
    protected abstract MeshData GenerateForTiles(Tiff tiffImage, TiffMetadata info, int downsample);

    /// <summary>
    ///     Checks of the Tiff image can be used as a height map for generating meshes.
    ///     For use as a height map, the image must be in 16-bit or 32-bit grayscale.
    ///     Both scanline and tiled encoding are support.
    ///     Throws an error if an invalid or incompatible Tiff image is used.
    /// </summary>
    protected void ValidateTiff(TiffMetadata info) {

        if (info == null) {
            throw new Exception("TIFF file cannot be null."); // TODO Create exception for this.
        }

        // Currently, only 16-bit and 32-bit grayscale files are supported.
        if (info.BPP != 32 && info.BPP != 16 || info.SPP != 1) {
            throw new FileFormatException("Invalid TIFF format. Only 16-bit and 32-bit grayscale files are supported.");
        }

    }

}
