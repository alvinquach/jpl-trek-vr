namespace TrekVRApplication {

    /// <summary>
    ///     Used for relaying layer adjustments and changes from the user 
    ///     interface to the layer controller(s).
    /// </summary>
    public class TerrainLayerChange {

        public int Index { get; set; }

        public float? Opacity { get; set; }

        public bool? Visible { get; set; }

    }

}
