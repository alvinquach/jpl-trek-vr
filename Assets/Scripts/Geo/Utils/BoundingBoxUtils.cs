using System;
using UnityEngine;

namespace TrekVRApplication {

    public static class BoundingBoxUtils {

        /// <summary>
        ///     Parses a comma delimted string contaning with start longitude,
        ///     start latitude, end longitude, and end latitude values.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static BoundingBox ParseBoundingBox(string boundingBox) {
            string[] split = boundingBox.Split(',');
            // TODO Add sanity checks.
            return new BoundingBox(
                float.Parse(split[0]),
                float.Parse(split[1]),
                float.Parse(split[2]),
                float.Parse(split[3])
            );
        }

        /// <summary>
        ///     Calculates the median latitude and longitude of a bounding box.
        ///     Returns a Vector2 where x is the latitude and y is the longitude.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns>A Vector2 where x is the latitude and y is the longitude.</returns>
        public static Vector2 MedianLatLon(BoundingBox boundingBox) {
            float lon = (boundingBox[0] + boundingBox[2]) / 2;

            // Handle case where the start and end longitudes have opposite signs.
            if (ReverseLonOrder(boundingBox)) {
                lon += (lon > 0 ? -180 : 180);
            }

            return new Vector2((boundingBox[1] + boundingBox[3]) / 2, lon);
        }

        public static Vector3 MedianDirection(BoundingBox boundingBox) {
            Vector2 medianLatLong = MedianLatLon(boundingBox);
            Debug.Log(medianLatLong);
            return CoordinateUtils.LatLonToDirection(medianLatLong);
        }

        /// <summary>
        ///     Calculates the largest dimension (width or height) of the terrain slice
        ///     enclosed by the bounding box if the terrain slice is rotated such that
        ///     the median direction is pointing upwards in world space.
        /// </summary>
        /// <param name="boundingBox"></param>
        public static float LargestDimension(BoundingBox boundingBox, float radius = 1.0f) {

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

        public static bool ReverseLonOrder(BoundingBox boundingBox) {
            return boundingBox[0] * boundingBox[2] < 0 && Mathf.Abs(boundingBox[0] - boundingBox[2]) > 180.0f;
        }

        [Obsolete]
        public static Vector4 SortBoundingBox(Vector4 boundingBox) {
            Vector4 result = boundingBox;
            if (result[3] < result[1]) {
                result[1] = boundingBox[3];
                result[3] = boundingBox[1];
            }
            if (result[2] < result[0]) {
                result[0] = boundingBox[2];
                result[2] = boundingBox[0];
            }
            return result;
        }

    }

}