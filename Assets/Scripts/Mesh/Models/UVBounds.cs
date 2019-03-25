using System;
using UnityEngine;

namespace TrekVRApplication {

    public struct UVBounds {

        public static UVBounds Default { get => new UVBounds(0f, 0f, 1f, 1f); }

        public float U1 { get; set; }
        public float V1 { get; set; }
        public float U2 { get; set; }
        public float V2 { get; set; }

        public float this[int index] {
            get {
                switch (index) {
                    case 0: return U1;
                    case 1: return V1;
                    case 2: return U2;
                    case 3: return V2;
                    default:
                        throw new IndexOutOfRangeException("Invalid UVBounds index!");
                }
            }
            set {
                switch (index) {
                    case 0: U1 = value; break;
                    case 1: V1 = value; break;
                    case 2: U2 = value; break;
                    case 3: V2 = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid UVBounds index!");
                }
            }
        }

        public UVBounds(float u1, float v1, float u2, float v2) {
            U1 = u1;
            V1 = v1;
            U2 = u2;
            V2 = v2;
        }

        public UVBounds(Vector4 v) : this(v[0], v[1], v[2], v[3]) {

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

        public override bool Equals(object obj) {
            if (!(obj is UVBounds)) {
                return false;
            }

            var bounds = (UVBounds)obj;
            return U1 == bounds.U1 &&
                   V1 == bounds.V1 &&
                   U2 == bounds.U2 &&
                   V2 == bounds.V2;
        }

        public override int GetHashCode() {
            var hashCode = 1659731994;
            hashCode = hashCode * -1521134295 + U1.GetHashCode();
            hashCode = hashCode * -1521134295 + V1.GetHashCode();
            hashCode = hashCode * -1521134295 + U2.GetHashCode();
            hashCode = hashCode * -1521134295 + V2.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(UVBounds bbox1, UVBounds bbox2) {
            return bbox1.Equals(bbox2);
        }

        public static bool operator !=(UVBounds bbox1, UVBounds bbox2) {
            return !bbox1.Equals(bbox2);
        }

        // Converts a Vector4 to UVBounds.
        public static implicit operator UVBounds(Vector4 v) {
            return new UVBounds(v);
        }

        // Converts a Vector4 to UVBounds.
        public static implicit operator Vector4(UVBounds bbox) {
            return new Vector4(bbox[0], bbox[1], bbox[2], bbox[3]);
        }

    }

}
