
namespace TrekVRApplication {

    public class LoadImageFromFileTask : ThreadedTask<float, BGRAImage> {

        private string _filepath;

        private float _progress = 0.0f;

        public int TextureWidth { get; private set; }

        public int TextureHeight { get; private set; }

        public LoadImageFromFileTask(string filepath) {
            _filepath = filepath;
        }

        public override float GetProgress() {
            return _progress;
        }

        protected sealed override BGRAImage Task() {

            BGRAImage srcImage;

            // TODO Support other file types.
            // TODO Check if path is valid.
            using (TiffWrapper tiff = new TiffWrapper(_filepath)) {
                srcImage = tiff.ToBGRAImage();
            }

            TextureWidth = srcImage.Width;
            TextureHeight = srcImage.Height;

            return srcImage;

            //return TextureUtils.GenerateMipmaps(srcImage);

            //return TextureToolUtils.ImageToTexture(srcImage, _textureFormat);
        }

    }

}
