
namespace TrekVRApplication {

    public class LoadColorImageFromFileTask<T> : LoadImageFromFileTask<T> where T : ColorImage {

        public LoadColorImageFromFileTask(string filepath) : base(filepath) {

        }

        protected override T GetImage(TiffImage tiff) {
            return TiffImageConverter.ToColorImage<T>(tiff);
        }

    }

}
