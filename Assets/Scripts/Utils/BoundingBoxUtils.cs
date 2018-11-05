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

    //public static Vector3 AverageDirection(Vector4 boundingBox) {

    //}

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