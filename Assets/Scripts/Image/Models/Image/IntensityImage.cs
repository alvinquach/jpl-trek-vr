using System;

namespace TrekVRApplication {

    public class IntensityImage : Image<float, float> {

        protected override int DataPerPixel {
            get { return 1; }
        }

        public IntensityImage(int width, int height) : base(width, height) {

        }

        public override void SetPixel(int x, int y, float value) {
            if (IsOutOfBounds(x, y)) {
                return;
            }
            long offset = GetPixelOffet(x, y);
            _rawData[offset] = value;
        }

        public override float GetPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            if (!AdjustCoordinates(ref x, ref y, boundaryMode)) {
                return default;
            }
            long offset = GetPixelOffet(x, y);
            return _rawData[offset];
        }

        public override float[] GetRawPixel(int x, int y, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            return new float[] { GetPixel(x, y, boundaryMode) };
        }

        public override float GetAverage(int x, int y, int width, int height, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {

            float sum = 0;
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
                    sum += GetPixel(i, j, boundaryMode);
                    count++;
                }
            }

            return sum / count;
        }

    }

}