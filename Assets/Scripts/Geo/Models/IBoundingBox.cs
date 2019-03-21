namespace TrekVRApplication {

    public interface IBoundingBox {

        float LonStart { get; set; }

        float LatStart { get; set; }

        float LonEnd { get; set; }

        float LatEnd { get; set; }

        float LonSwing { get; }

        float LatSwing { get; }

        float this[int index] { get; set; }

        string ToString(string delimiter, int decimalPlaces = 4);

    }

}