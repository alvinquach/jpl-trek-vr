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
        public static Vector2 MedianLatLon(IBoundingBox boundingBox) {
            float lat = (boundingBox[1] + boundingBox[3]) / 2;
            float lon = MathUtils.WrapAngle180(boundingBox[0] + boundingBox.LonSwing / 2);
            return new Vector2(lat, lon);
        }

        public static Vector3 MedianDirection(BoundingBox boundingBox) {
            Vector2 medianLatLong = MedianLatLon(boundingBox);
            return CoordinateUtils.LatLonToDirection(medianLatLong);
        }

        public static BoundingBox ExpandToSquare(BoundingBox boundingBox) {
            try {
                return ExpandToSquare(boundingBox, RelativePosition.TopLeft);
            }
            catch (Exception) {
                boundingBox.LatStart = -90.0f;
                return ExpandToSquare(boundingBox, RelativePosition.BottomRight);
            }
        }

        public static BoundingBox ExpandToSquare(BoundingBox boundingBox, RelativePosition relativeTo = RelativePosition.Center) {
            float totalLon = boundingBox.LonSwing;
            float totalLat = boundingBox.LatSwing;
            if (totalLon == totalLat) {
                return boundingBox;
            }

            // Expand vertically
            if (totalLon > totalLat) {

                float latStart = boundingBox.LatStart;
                float latEnd = boundingBox.LatEnd;
                float diff = totalLon - totalLat;

                switch (relativeTo) {
                    case RelativePosition.TopLeft:
                    case RelativePosition.Top:
                    case RelativePosition.TopRight:
                        latStart -= diff;
                        break;
                    case RelativePosition.Left:
                    case RelativePosition.Center:
                    case RelativePosition.Right:
                        latStart -= diff / 2;
                        latEnd += diff / 2;
                        break;
                    case RelativePosition.BottomLeft:
                    case RelativePosition.Bottom:
                    case RelativePosition.BottomRight:
                        latEnd += diff;
                        break;
                }

                // Check if the start latitude or end latitude are out of bounds.
                if (latStart < -90.0f) {
                    throw new Exception("Start latitude out of range.");
                }
                if (latEnd > 90.0f) {
                    throw new Exception("End latitude out of range.");
                }

                return new BoundingBox(boundingBox.LonStart, latStart, boundingBox.LonEnd, latEnd);
            }

            // Expand horizontally
            else {

                float lonStart = boundingBox.LonStart;
                float lonEnd = boundingBox.LonEnd;
                float diff = totalLat - totalLon;

                switch (relativeTo) {
                    case RelativePosition.TopLeft:
                    case RelativePosition.Left:
                    case RelativePosition.BottomLeft:
                        lonEnd += diff;
                        break;
                    case RelativePosition.Top:
                    case RelativePosition.Center:
                    case RelativePosition.Bottom:
                        lonEnd += diff / 2;
                        lonStart -= diff / 2;
                        break;
                    case RelativePosition.TopRight:
                    case RelativePosition.Right:
                    case RelativePosition.BottomRight:
                        lonStart -= diff;
                        break;
                }

                return new BoundingBox(lonStart, boundingBox.LatStart, lonEnd, boundingBox.LatEnd);
            }

        }

        public static UVBounds CalculateUVBounds(IBoundingBox boundingBox, IBoundingBox selectedArea) {
            if (boundingBox == selectedArea) {
                return UVBounds.Default;
            }
            float uStart = (selectedArea.LonStart - boundingBox.LonStart) / boundingBox.LonSwing;
            float uEnd = 1 - (boundingBox.LonEnd - selectedArea.LonEnd) / boundingBox.LonSwing;
            float vStart = (boundingBox.LatEnd - selectedArea.LatEnd) / boundingBox.LatSwing;
            float vEnd = 1 - (selectedArea.LatStart - boundingBox.LatStart) / boundingBox.LatSwing;
            return new UVBounds(uStart, vStart, uEnd, vEnd);
        }

        public static UVScaleOffset CalculateUVScaleOffset(IBoundingBox boundingBox, IBoundingBox selectedArea) {
            selectedArea = UnwrapRight(selectedArea);
            return CalculateUVScaleOffset(CalculateUVBounds(boundingBox, selectedArea));
        }

        public static UVScaleOffset CalculateUVScaleOffset(UVBounds relativeUV) {
            if (relativeUV == UVBounds.Default) {
                return UVScaleOffset.Default;
            }

            return new UVScaleOffset() {
                Scale = new Vector2(
                    relativeUV.U2 - relativeUV.U1,
                    relativeUV.V2 - relativeUV.V1
                ),
                Offset = new Vector2(
                    relativeUV.U1,
                    relativeUV.V1
                )
            };
        }

        /// <summary>
        ///     Calculates the latitude and longitude from a UV coordinate and a
        ///     corresponding bounding box.
        /// </summary>
        /// <returns>A Vector2 where x is the latitude and y is the longitude.</returns>
        public static Vector2 UVToCoordinates(IBoundingBox boundingBox, Vector2 uv) {
            return new Vector2(
                boundingBox.LatStart + (1 - uv.y) * boundingBox.LatSwing,
                boundingBox.LonStart + uv.x * boundingBox.LonSwing
            );
        }

        /// <summary>
        ///     Calculates the UV coordinate from a latitude and longitude and a
        ///     corresponding bounding box.
        /// </summary>
        public static Vector2 CoordinatesToUV(IBoundingBox boundingBox, Vector2 latLon) {
            // TODO Test this...
            return new Vector2(
                (latLon.y - boundingBox.LonStart) / boundingBox.LonSwing,
                (boundingBox.LatEnd - latLon.x) / boundingBox.LatSwing
            );
        }

        /// <summary>
        ///     Calculates the largest dimension (width or height) of the terrain slice
        ///     enclosed by the bounding box if the terrain slice is rotated such that
        ///     the median direction is pointing upwards in world space. Currently only
        ///     works with restricted (total longitude <= 180°) bounding boxes.
        /// </summary>
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

        /// <summary>
        ///     Checks wheter the bounding box crosses the +/- 180° longitude line.
        /// </summary>
        public static bool IsLongitudeWrapped(IBoundingBox bbox) {
            return bbox[0] > bbox[2];
        }

        public static IBoundingBox UnwrapLeft(IBoundingBox bbox) {
            if (!IsLongitudeWrapped(bbox)) {
                return bbox;
            }
            return new UnrestrictedBoundingBox(bbox.LonEnd - bbox.LonSwing, bbox[1], bbox[2], bbox[3]);
        }

        public static IBoundingBox UnwrapRight(IBoundingBox bbox) {
            if (!IsLongitudeWrapped(bbox)) {
                return bbox;
            }
            return new UnrestrictedBoundingBox(bbox[0], bbox[1], bbox.LonStart + bbox.LonSwing, bbox[3]);
        }

    }

}