
using System;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     A geographic bounding box that allows longitudinal angle sweeps greater
    ///     than 180°. Also does not limit longitude extremes to +/- 180°.
    /// </summary>
    public struct UnrestrictedBoundingBox : IEquatable<IBoundingBox>, IBoundingBox {

        public static UnrestrictedBoundingBox Zero { get => new UnrestrictedBoundingBox(0f, 0f, 0f, 0f); } 

        public static UnrestrictedBoundingBox Global { get => new UnrestrictedBoundingBox(-180f, -90f, 180f, 90f); } 

        private float _lonStart;
        public float LonStart {
            get => _lonStart;
            set {
                _lonStart = value;
                SortBoundingBox();
            }
        }

        private float _latStart;
        public float LatStart {
            get => _latStart;
            set {
                _latStart = WrapLatitude(value);
                SortBoundingBox();
            }
        }

        private float _lonEnd;
        public float LonEnd {
            get => _lonEnd;
            set {
                _lonEnd = value;
                SortBoundingBox();
            }
        }

        private float _latEnd;
        public float LatEnd {
            get => _latEnd;
            set {
                _latEnd = WrapLatitude(value);
                SortBoundingBox();
            }
        }
        public float LonSwing {
            get => this[0] > this[2] ? 360 + this[2] - this[0] : this[2] - this[0];
        }
        public float LatSwing {
            get => this[3] - this[1];
        }

        public float this[int index] {
            get {
                switch (index) {
                    case 0: return _lonStart;
                    case 1: return _latStart;
                    case 2: return _lonEnd;
                    case 3: return _latEnd;
                    default:
                        throw new IndexOutOfRangeException("Invalid BoundingBox index!");
                }
            }
            set {
                switch (index) {
                    case 0: LonStart = value; break;
                    case 1: LatStart = value; break;
                    case 2: LonEnd = value; break;
                    case 3: LatEnd = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid BoundingBox index!");
                }
            }
        }

        public UnrestrictedBoundingBox(float lonStart, float latStart, float lonEnd, float latEnd) {
            _lonStart = lonStart;
            _latStart = WrapLatitude(latStart);
            _lonEnd = lonEnd;
            _latEnd = WrapLatitude(latEnd);
            SortBoundingBox();
        }

        public UnrestrictedBoundingBox(Vector4 v) : this(v[0], v[1], v[2], v[3]) {

        }

        public UnrestrictedBoundingBox(IBoundingBox bbox) : this(bbox[0], bbox[1], bbox[2], bbox[3]) {

        }

        private void SortBoundingBox() {
            if (this[2] < this[0]) {
                float lon = this[0];
                this[0] = this[2];
                this[2] = lon;
            }
            if (this[3] < this[1]) {
                float lat = this[1];
                this[1] = this[3];
                this[3] = lat;
            }
        }

        public bool Equals(IBoundingBox other) {
            return _lonStart == other.LonStart
                && _latStart == other.LatStart
                && _lonEnd == other.LonEnd
                && _latEnd == other.LatEnd;
        }

        public override bool Equals(object obj) {
            if (!(obj is IBoundingBox)) {
                return false;
            }
            return Equals((IBoundingBox)obj);
        }

        public override int GetHashCode() {
            var hashCode = -335603062;
            hashCode = hashCode * -1521134295 + _lonStart.GetHashCode();
            hashCode = hashCode * -1521134295 + _latStart.GetHashCode();
            hashCode = hashCode * -1521134295 + _lonEnd.GetHashCode();
            hashCode = hashCode * -1521134295 + _latEnd.GetHashCode();
            return hashCode;
        }

        public override string ToString() {
            return ToString(", ");
        }

        public string ToString(string delimiter, int decimalPlaces = 4) {

            // This will add thousands separators, but it shouldn't matter for
            // bounding box since the highest possible value should be 180.
            string format = $"n{decimalPlaces}"; 

            return this[0].ToString(format) + delimiter +
                   this[1].ToString(format) + delimiter +
                   this[2].ToString(format) + delimiter +
                   this[3].ToString(format);
        }

        private static float WrapLatitude(float lat) {
            return Mathf.Clamp(MathUtils.WrapAngle180(lat), -90f, 90f);
        }

        public static bool operator ==(UnrestrictedBoundingBox bbox1, IBoundingBox bbox2) {
            return bbox1.Equals(bbox2);
        }

        public static bool operator !=(UnrestrictedBoundingBox bbox1, IBoundingBox bbox2) {
            return !bbox1.Equals(bbox2);
        }

        public static bool operator ==(IBoundingBox bbox1, UnrestrictedBoundingBox bbox2) {
            return bbox1.Equals(bbox2);
        }

        public static bool operator !=(IBoundingBox bbox1, UnrestrictedBoundingBox bbox2) {
            return !bbox1.Equals(bbox2);
        }

        // Converts a Vector4 to a UnrestrictedBoundingBox.
        public static implicit operator UnrestrictedBoundingBox(Vector4 v) {
            return new UnrestrictedBoundingBox(v);
        }

        // Converts a BoundingBox to a UnrestrictedBoundingBox.
        public static implicit operator UnrestrictedBoundingBox(BoundingBox bbox) {
            return new UnrestrictedBoundingBox((IBoundingBox)bbox);
        }

        // Converts a UnrestrictedBoundingBox to a Vector4.
        public static implicit operator Vector4(UnrestrictedBoundingBox bbox) {
            return new Vector4(bbox[0], bbox[1], bbox[2], bbox[3]);
        }

        // Converts a UnrestrictedBoundingBox to a BoundingBox.
        public static implicit operator BoundingBox(UnrestrictedBoundingBox bbox) {
            return new BoundingBox((IBoundingBox)bbox);
        }

    }

}