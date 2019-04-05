
namespace TrekVRApplication {

    public class LoadIntensityImageFromFileTask : LoadImageFromFileTask<IntensityImage> {

        public LoadIntensityImageFromFileTask(string filepath) : base(filepath) {

        }

        protected override void VerifyImageFormat(TiffImage tiff) {
            base.VerifyImageFormat(tiff);

            // For use as a height map, the image must be in 16-bit or 32-bit grayscale.
            // Both scanline and tiled encoding are supported.
            if (tiff.Metadata.BitsPerSample != 32 && tiff.Metadata.BitsPerSample != 16 || tiff.Metadata.SamplesPerPixel != 1) {
                throw new FileFormatException("Invalid TIFF format. Only 16-bit and 32-bit grayscale files are supported.");
            }
        }

        protected override IntensityImage FromTiffImage(TiffImage tiff) {
            return TiffImageLoader.ToIntensityImage(tiff);
        }

    }

}
