using System;
using UnityEngine;

namespace TrekVRApplication {

    public abstract class GenerateTerrainMeshFromDigitalElevationModelTask : GenerateTerrainMeshTask {

        private string[] _demFilePaths;

        public GenerateTerrainMeshFromDigitalElevationModelTask(string[] demFilePaths, TerrainModelMeshMetadata metadata) : base(metadata) {
            _demFilePaths = demFilePaths;
        }

        // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.

        /// <summary>
        ///     Generates the mesh data and store it in the member variable.
        /// </summary>
        protected override void Generate() {

            // TODO Add support other image types if necessary.

            // Create and execute task to convert first image into an RGBImage object.
            // The task can be executed syncronously in the same thread since Generate() 
            // is called outside of the main thread.
            IntensityImage baseImage = new LoadIntensityImageFromFileTask(_demFilePaths[0]).ExecuteInCurrentThread();

            // If there are more files to process, then load and merge each image.
            for (int i = 1; i < _demFilePaths.Length; i++) {

                // Load the additional image.
                IntensityImage image = new LoadIntensityImageFromFileTask(_demFilePaths[i]).ExecuteInCurrentThread();

                // Merge the images together.
                try {
                    baseImage.Merge(image, -32768);

                    // NOTE: -32768 happens to be the "blank" value for the global DEM (and probably
                    // any other files in 16-bit integer). This may not work for files in other formats.
                    // Update if necessary.
                }
                catch (Exception e) {
                    Debug.LogError(e.Message);
                    continue;
                }
            }

            TerrainMeshData[] meshData = new TerrainMeshData[_metadata.TotalLodLevels];

            for (int lodLevel = 0; lodLevel <= _metadata.LodLevels; lodLevel++) {
                int downsample = 1 << (_metadata.BaseDownsample + lodLevel);
                meshData[lodLevel] = GenerateForLod(baseImage, downsample);
            }

            if (_metadata.GenerateAdditionalPhysicsLod) {
                meshData[meshData.Length - 1] = GenerateForLod(baseImage, 1 << _metadata.PhysicsDownsample);
            }

            _progress = 1.0f;
            _meshData = meshData;

        }

        /*
         * TODO Modify the implementations of the function such that
         * they generate all the LODs at the same time, instead of
         * having to read the image data once for each LOD.
         */
        protected abstract TerrainMeshData GenerateForLod(IntensityImage image, int downsample);

    }

}
