
using System;
using UnityEngine;

namespace App.Geo {

    public struct BoundingBox {

        private float _lonStart;
        public float LonStart {
            get { return _lonStart; }
            set {
                _lonStart = WrapLongitude(value);
                SortBoundingBox();
            }
        }

        private float _latStart;
        public float LatStart {
            get { return _latStart; }
            set {
                _latStart = WrapLatitude(value);
                SortBoundingBox();
            }
        }

        private float _lonEnd;
        public float LonEnd {
            get { return _lonEnd; }
            set {
                _lonEnd = WrapLongitude(value);
                SortBoundingBox();
            }
        }

        private float _latEnd;
        public float LatEnd {
            get { return _latEnd; }
            set {
                _latEnd = WrapLatitude(value);
                SortBoundingBox();
            }
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

        public BoundingBox(float lonStart, float latStart, float lonEnd, float latEnd) {
            _lonStart = WrapLongitude(lonStart);
            _latStart = WrapLatitude(latStart);
            _lonEnd = WrapLongitude(lonEnd);
            _latEnd = WrapLatitude(lonEnd);
            SortBoundingBox();
        }

        public BoundingBox(Vector4 v) : this(v[0], v[1], v[2], v[3]) {

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

        private static float WrapLongitude(float lon) {
            return MathUtils.WrapAngle180(lon);
        }

        private static float WrapLatitude(float lat) {
            return Mathf.Clamp(MathUtils.WrapAngle180(lat), -90f, 90f);
        }

        public static BoundingBox Zero { get { return new BoundingBox(0f, 0f, 0f, 0f); } }

        // Converts a Vector4 to a BoundingBox.
        public static implicit operator BoundingBox(Vector4 v) {
            return new BoundingBox(v);
        }

        // Converts a Vector4 to a BoundingBox.
        public static implicit operator Vector4(BoundingBox bbox) {
            return new Vector4(bbox[0], bbox[1], bbox[2], bbox[3]);
        }

    }

}