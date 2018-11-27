using System;

namespace TrekVRApplication {

    public abstract class GenerateDigitalElevationModelTerrainMeshTask : GenerateTerrainMeshTask {

        public GenerateDigitalElevationModelTerrainMeshTask(TerrainModelMetadata metadata) : base(metadata) {

        }

        // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.

        /// <summary>Generate the mesh data and store it in the member variable.</summary>
        protected override void Generate() {

            // TODO Add support other image types if necessary.

            // Use TIFF as source for DEM data.
            using (TiffWrapper tiffImage = new TiffWrapper(_metadata.demFilePath)) {

                /*
                 * Checks of the Tiff image can be used as a height map for generating meshes.
                 * For use as a height map, the image must be in 16-bit or 32-bit grayscale.
                 * Both scanline and tiled encoding are supported.
                 */
                if (!tiffImage || !tiffImage.Metadata) {
                    throw new Exception("TIFF file cannot be null.");
                }
                if (tiffImage.Metadata.BPP != 32 && tiffImage.Metadata.BPP != 16 || tiffImage.Metadata.SPP != 1) {
                    throw new FileFormatException("Invalid TIFF format. Only 16-bit and 32-bit grayscale files are supported.");
                }

                MeshData[] meshData = new MeshData[_metadata.lodLevels + 1];

                Image<float> image = tiffImage.ToIntensityImage();

                for (int lodLevel = 0; lodLevel <= _metadata.lodLevels; lodLevel++) {
                    int downsample = _metadata.baseDownsample << lodLevel;
                    meshData[lodLevel] = GenerateForLod(image, downsample);
                }

                _progress = 1.0f;
                _meshData = meshData;
            }

        }

        /*
         * TODO Modify the implementations of the function such that
         * they generate all the LODs at the same time, instead of
         * having to read the image data once for each LOD.
         */
        protected abstract MeshData GenerateForLod(Image<float> image, int downsample);

    }

}
