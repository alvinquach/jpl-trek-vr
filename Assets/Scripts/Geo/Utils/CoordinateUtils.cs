using UnityEngine;

namespace TrekVRApplication {

    public static class CoordinateUtils {

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

        /// <summary>
        ///     Converts a Vector2 in the format of (lat, lon) to a point in
        ///     3D space relative to the center of an object. The distance from
        ///     the center of the object to the point will always be 1.
        /// </summary>
        /// <param name="latLon">(latitude, longitude)</param>
        /// <param name="lonOffset">Longitude offset in degrees.</param>
        /// <returns></returns>
        public static Vector3 LatLonToPosition(Vector2 latLon, float radius = 1.0f, float lonOffset = 0.0f) {

            // This is actually the same as finding direction relative to center.
            return radius * LatLonToDirection(latLon, lonOffset);

        }

    }

}