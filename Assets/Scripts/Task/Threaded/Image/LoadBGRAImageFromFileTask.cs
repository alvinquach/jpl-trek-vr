
namespace TrekVRApplication {

    public class LoadBGRAImageFromFileTask : LoadImageFromFileTask<BGRAImage> {

        public LoadBGRAImageFromFileTask(string filepath) : base(filepath) {

        }

        protected override BGRAImage GetImage(TiffWrapper tiff) {
            return tiff.ToBGRAImage();
        }

    }

}
