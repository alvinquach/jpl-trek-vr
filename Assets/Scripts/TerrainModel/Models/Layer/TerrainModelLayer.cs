namespace TrekVRApplication {

    public struct TerrainModelLayer {

        /// <summary>
        ///     Name of the layer for display only. Not used for identification purposes.
        /// </summary>
        public string Name { get; }


        /// <summary>
        ///     The UUID of the product used by the layer.
        /// </summary>
        public string ProductUUID { get; }

        public float Opacity { get; set; }

        public bool Visible { get; set; }

        public bool Editable { get; }

        public TerrainModelLayer(string name, string uuid, bool editable = true) {
            Name = name;
            ProductUUID = uuid;
            Editable = editable;
            Opacity = 1.0f;
            Visible = true;
        }

    }

}