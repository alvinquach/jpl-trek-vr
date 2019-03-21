using UnityEngine;

namespace TrekVRApplication {

    public struct UVScaleOffset {

        public static UVScaleOffset Default {
            get {
                return new UVScaleOffset() {
                    Scale = Vector2.one,
                    Offset = Vector2.zero
                };
            }
        }

        public Vector2 Scale { get; set; }

        public Vector2 Offset { get; set; }

    }

}