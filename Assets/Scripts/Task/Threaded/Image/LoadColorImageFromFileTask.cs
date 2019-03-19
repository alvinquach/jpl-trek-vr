
namespace TrekVRApplication {

    public class LoadColorImageFromFileTask<T> : LoadImageFromFileTask<T> where T : ColorImage {

        public LoadColorImageFromFileTask(string filepath) : base(filepath) {

        }

        protected override T FromTiffImage(TiffImage tiff) {
            return TiffImageLoader.ToColorImage<T>(tiff);
        }

    }

}
