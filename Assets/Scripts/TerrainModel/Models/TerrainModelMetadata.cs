namespace TrekVRApplication {

    public struct TerrainModelMetadata {

        public string DemFilePath { get; set; }

        public float Radius { get; set; }

        public float HeightScale { get; set; }

        public int LodLevels { get; set; }

        public int BaseDownsample { get; set; }

        public float heightScale;

    }

}