using System;
using UnityEngine;

namespace TrekVRApplication {

    public class BGRAImage : Image<Color32, byte> {

        protected override int DataPerPixel {
            get { return 4; }
        }

        public BGRAImage(int width, int height) : base(width, height) {

        }

        public override void SetPixel(int x, int y, Color32 value) {
            if (IsOutOfBounds(x, y)) {
                return;
            }
            long offset = GetPixelOffet(x, y);
            _rawData[offset] = value.b;
            _rawData[offset + 1] = value.g;
            _rawData[offset + 2] = value.r;
            _rawData[offset + 3] = value.a;
        }

        public override Color32 GetPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            if (!AdjustCoordinates(ref x, ref y, boundaryMode)) {
                return default;
            }
            long offset = GetPixelOffet(x, y);
            return new Color32(
                _rawData[offset + 2],
                _rawData[offset + 1],
                _rawData[offset],
                _rawData[offset + 3]
            );
        }

        public override byte[] GetRawPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            if (!AdjustCoordinates(ref x, ref y, boundaryMode)) {
                return default;
            }
            long offset = GetPixelOffet(x, y);
            byte[] pixel = new byte[DataPerPixel];
            Array.Copy(_rawData, offset, pixel, 0, DataPerPixel);
            return pixel;
        }

        public override Color32 GetAverage(int x, int y, int width, int height, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            int[] sum = new int[] { 0, 0, 0, 0 };
            int count = 0;

            for (int j = y; j < y + height; j++) {

                // If the boundary mode is 'None', then ignore the entire row if it is out of bounds.
                if (boundaryMode == ImageBoundaryMode.None && IsOutOfBounds(0, j)) {
                    continue;
                }

                for (int i = x; i < x + width; i++) {

                    // If the boundary mode is 'None', then ignore the pixel if it is out of bounds.
                    if (boundaryMode == ImageBoundaryMode.None && IsOutOfBounds(i, j)) {
                        continue;
                    }

                    // Call GetPixel() instead of accessing the pixel array directly to handle boundaries.
                    byte[] pixel = GetRawPixel(i, j, boundaryMode);
                    for (int k = 0; k < 4; k++) {
                        sum[k] += pixel[k];
                    }
                    count++;
                }
            }

            return new Color32(
                (byte) (sum[2] / count),
                (byte) (sum[1] / count),
                (byte) (sum[0] / count),
                (byte) (sum[3] / count)
            );

        }
    }

}
