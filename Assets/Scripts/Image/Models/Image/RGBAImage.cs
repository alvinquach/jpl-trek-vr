using System;
using UnityEngine;

namespace TrekVRApplication {

    [Obsolete("Use BGRAImage instead.")]
    public class RGBAImage : ColorImage {

        protected override int DataPerPixel {
            get => 4;
        }

        public RGBAImage(int width, int height) : base(width, height) {

        }

        public override void SetPixel(int x, int y, Color32 value) {
            if (IsOutOfBounds(x, y)) {
                return;
            }
            long offset = GetPixelOffet(x, y);
            _rawData[offset] = value.r;
            _rawData[offset + 1] = value.g;
            _rawData[offset + 2] = value.b;
            _rawData[offset + 3] = value.a;
        }

        public override Color32 GetPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            if (!AdjustCoordinates(ref x, ref y, boundaryMode)) {
                return default;
            }
            long offset = GetPixelOffet(x, y);
            return new Color32(
                _rawData[offset],
                _rawData[offset + 2],
                _rawData[offset + 1],
                _rawData[offset + 3]
            );
        }

        public override Color32 GetAverage(int x, int y, int width, int height, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            // TODO Implement this.
            throw new NotImplementedException();
        }

    }

}
