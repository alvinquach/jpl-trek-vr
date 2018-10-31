using UnityEngine;

public sealed class CoordinateUtils {
    
    private CoordinateUtils() { }

    /// <summary>
    ///     Converts a Vector2 in the format of (lat, lon) to a direction
    ///     vector relative to the center of an object.
    /// </summary>
    /// <param name="latLon">(latitude, longitude)</param>
    /// <param name="lonOffset">Longitude offset in degrees.</param>
    /// <returns></returns>
    public static Vector3 LatLonToDirection(Vector2 latLon, float lonOffset = 0.0f) {

        return Quaternion.Euler(-latLon.x, -latLon.y - lonOffset, 0) * Vector3.forward;

    }

}