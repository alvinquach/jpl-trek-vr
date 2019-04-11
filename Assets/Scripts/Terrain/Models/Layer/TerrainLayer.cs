namespace TrekVRApplication {

    public struct TerrainLayer {

        /// <summary>
        ///     Name of the layer for display only. Not used for identification purposes.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     For display only.
        /// </summary>
        public string ThumbnailUrl { get; }

        /// <summary>
        ///     The UUID of the product used by the layer.
        /// </summary>
        public string ProductUUID { get; }

        public float Opacity { get; set; }

        public bool Visible { get; set; }


        public TerrainLayer(string name, string uuid) : this(name, uuid, null) {
            
        }

        public TerrainLayer(string name, string uuid, string thumbnailUrl) {
            Name = name;
            ThumbnailUrl = thumbnailUrl;
            ProductUUID = uuid;
            Opacity = 1.0f;
            Visible = true;
        }

    }

}