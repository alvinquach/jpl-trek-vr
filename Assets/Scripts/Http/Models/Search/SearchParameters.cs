namespace TrekVRApplication {

    public struct SearchParameters {

        public string Search { get; set; }

        public SearchItemType ItemType { get; set; }

        public string ProductType { get; set; }

        public string Mission { get; set; }

        public string Instrument { get; set; }

        public int? Limit { get; set; }

        public string BoundingBox { get; set; }

    }

}
