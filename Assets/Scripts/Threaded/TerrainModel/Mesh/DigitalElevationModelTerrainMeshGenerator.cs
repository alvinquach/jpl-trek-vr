using BitMiracle.LibTiff.Classic;
using System;

public abstract class DigitalElevationModelTerrainMeshGenerator : TerrainMeshGenerator {

    protected string _filepath;

    protected int _lodLevels;

    protected int _baseDownsample;

    protected float _heightScale;

    public DigitalElevationModelTerrainMeshGenerator(string filepath, float heightScale, int lodLevels, int baseDownsample) {

        // TODO Check if baseDownsample is power of 2.

        _filepath = filepath;
        _lodLevels = lodLevels;
        _heightScale = heightScale;
        _baseDownsample = baseDownsample;
    }

    // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.

    public override void Generate() {

        // TODO Add support other image types if necessary.

        // Use TIFF as source for DEM data.
        using (TiffWrapper tiffImage = new TiffWrapper(_filepath)) {

            // Checks of the Tiff image can be used as a height map for generating meshes.
            // For use as a height map, the image must be in 16-bit or 32-bit grayscale.
            // Both scanline and tiled encoding are supported.
            if (!tiffImage || !tiffImage.Metadata) {
                throw new Exception("TIFF file cannot be null.");
            }
            if (tiffImage.Metadata.BPP != 32 && tiffImage.Metadata.BPP != 16 || tiffImage.Metadata.SPP != 1) {
                throw new FileFormatException("Invalid TIFF format. Only 16-bit and 32-bit grayscale files are supported.");
            }

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

    // TODO Modify the implementations of the function such that
    // they generate all the LODs at the same time, instead of
    // having to read the image data once for each LOD.
    protected abstract MeshData Generate(Image<float> image, int downsample);


}
