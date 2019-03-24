﻿namespace TrekVRApplication {

    public abstract class GenerateTerrainMeshFromDigitalElevationModelTask : GenerateTerrainMeshTask {

        public GenerateTerrainMeshFromDigitalElevationModelTask(TerrainModelMetadata metadata) : base(metadata) {

        }

        // TODO Use a struct or class to pass DEM metadata (ie. scale) to the GenerateMesh methods.

        /// <summary>
        ///     Generates the mesh data and store it in the member variable.
        /// </summary>
        protected override void Generate() {

            // TODO Add support other image types if necessary.

            // Use TIFF as source for DEM data.
            IntensityImage image = new LoadIntensityImageFromFileTask(_metadata.DemFilePath).ExecuteInSameThread();

            MeshData[] meshData = new MeshData[_metadata.LodLevels + 1];

            for (int lodLevel = 0; lodLevel <= _metadata.LodLevels; lodLevel++) {
                int downsample = _metadata.BaseDownsample << lodLevel;
                meshData[lodLevel] = GenerateForLod(image, downsample);
            }

            _progress = 1.0f;
            _meshData = meshData;

        }

        /*
         * TODO Modify the implementations of the function such that
         * they generate all the LODs at the same time, instead of
         * having to read the image data once for each LOD.
         */
        protected abstract MeshData GenerateForLod(IntensityImage image, int downsample);

    }

}
