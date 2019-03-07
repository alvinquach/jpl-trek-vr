using UnityEngine;

namespace TrekVRApplication {

    public class RGBAImage : Image<Color32> {

        public RGBAImage(int width, int height) : base(width, height) { }

        public override Color32 GetAverage(int x, int y, int width, int height, ImageBoundaryMode boundaryMode = ImageBoundaryMode.None) {
            Color sum = new Color(0, 0, 0, 0);
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
                    Color32 pixel = GetPixel(i, j, boundaryMode);
                    sum[0] += pixel.r;
                    sum[1] += pixel.g;
                    sum[2] += pixel.b;
                    sum[3] += pixel.a;
                    count++;
                }
            }

            return sum / count;
        }

        public override byte[] ToByteArray() {
            byte[] bytes = new byte[Width * Height * 4];
            int index = 0;
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    Color32 pixel = _pixels[x, y];

                    // This is ouput in BGRA format to work with NVTT.
                    // TODO Add option parameters to output in other formats.
                    bytes[index] = pixel.b;
                    bytes[index + 1] = pixel.g;
                    bytes[index + 2] = pixel.r;
                    bytes[index + 3] = pixel.a;

                    index += 4;
                }
            }
            return bytes;
        }

        protected override Color32 DefaultValue() {
            return new Color32(0, 0, 0, 255);
        }

    }

}