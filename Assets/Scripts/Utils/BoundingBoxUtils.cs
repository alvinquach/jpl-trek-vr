using UnityEngine;

/// <summary>
///     A bounding box in this application should be a Vector4
///     in the format (lonStart, latStart, lonEnd, latEnd).
/// </summary>
public sealed class BoundingBoxUtils {

    private BoundingBoxUtils() { }

    /// <summary>
    ///     Calculates the median latitude and longitude of a bounding box.
    ///     Returns a Vector2 where x is the latitude and y is the longitude.
    /// </summary>
    /// <param name="boundingBox"></param>
    /// <returns>A Vector2 where x is the latitude and y is the longitude.</returns>
    public static Vector2 MedianLatLon(Vector4 boundingBox) {
        return 0.5f * new Vector2(
            boundingBox[1] + boundingBox[3],
            boundingBox[0] + boundingBox[2]
        );
    }

    public static Vector3 MedianDirection(Vector4 boundingBox) {
        return CoordinateUtils.LatLonToDirection(MedianLatLon(boundingBox));
    }

    /// <summary>
    ///     Calculates the largest dimension (width or height) of the terrain slice
    ///     enclosed by the bounding box if the terrain slice is rotated such that
    ///     the median direction is pointing upwards in world space.
    /// </summary>
    /// <param name="boundingBox"></param>
    public static float LargestDimension(Vector4 boundingBox, float radius = 1.0f) {

        float width, height, lat;

        // If the latitudes are on opposite sides, then we need to
        // calculate the width at the equator.
        if (boundingBox[1] * boundingBox[3] < 0) {
            lat = 0.0f;
        }

        // Else, use the latitude closest to the equator to calculate the width.
        else {
            lat = Mathf.Min(Mathf.Abs(boundingBox[1]), Mathf.Max(boundingBox[3]));
        }

        // Calculate the width.
        width = Vector3.Distance(
            CoordinateUtils.LatLonToPosition(new Vector2(lat, boundingBox[0]), radius),
            CoordinateUtils.LatLonToPosition(new Vector2(lat, boundingBox[2]), radius)
        );

        // Height should be calculated at the median longitude.
        float lon = (boundingBox[0] + boundingBox[2]) / 2;

        // Calculate the height.
        height = Vector3.Distance(
            CoordinateUtils.LatLonToPosition(new Vector2(boundingBox[1], lon), radius),
            CoordinateUtils.LatLonToPosition(new Vector2(boundingBox[3], lon), radius)
        );

        return Mathf.Max(width, height);
    }


    public static Vector4 ValidateBoundingBox(Vector4 boundingBox) {
        Vector4 result = 1 * boundingBox; // Convert to radians
        if (result.w < result.y) {
            result.y = boundingBox.w;
            result.w = boundingBox.y;
        }
        if (result.z < result.x) {
            result.x = boundingBox.z;
            result.z = boundingBox.x;
        }
        return result;
    }

}