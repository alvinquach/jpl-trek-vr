using UnityEngine;

namespace TrekVRApplication {

    public static class TerrainModelConstants {

        public const float TerrainModelScale = 2.5e-7f;

        public const int TerrainSectionDemTargetSize = 512;

        public const int TerrainSectionTextureTargetSize = 1024;

        public const int TerrainSectionPhysicsTargetDownsample = 3;

        public const float ShaderSmoothness = 0.31f;

        public const float ShaderMetallic = 0.31f;

        public const float NoTextureShaderSmoothness = 0.37f;

        public const float NoTextureShaderMetallic = 0.31f;

        public const float GlobeModelLODCoefficient = 0.25f;

        public const string GlobalMosaicUUID = "8bc9352d-ee73-4d1f-94b8-de5495fd8dfa";

        public const string GlobalDigitalElevationModelUUID = "1cc3cfbb-ac38-46d1-a3df-5fff16ca397e";

    }

}
