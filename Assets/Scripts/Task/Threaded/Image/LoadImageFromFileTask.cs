
using System;
using UnityEngine;

namespace TrekVRApplication {

    public abstract class LoadImageFromFileTask<T> : ThreadedTask<float, T> where T : Image {

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

        protected sealed override T Task() {

            T srcImage;

            // TODO Support other file types.
            // TODO Check if path is valid.
            using (TiffImage tiff = new TiffImage(_filepath)) {
                VerifyImageFormat(tiff);
                srcImage = GetImage(tiff);
            }

            TextureWidth = srcImage.Width;
            TextureHeight = srcImage.Height;

            return srcImage;

            //return TextureUtils.GenerateMipmaps(srcImage);

            //return TextureToolUtils.ImageToTexture(srcImage, _textureFormat);
        }

        protected virtual void VerifyImageFormat(TiffImage tiff) {
            if (!tiff) {
                throw new Exception("TIFF file cannot be null.");
            }
        }

        protected abstract T GetImage(TiffImage tiff);

    }

}
