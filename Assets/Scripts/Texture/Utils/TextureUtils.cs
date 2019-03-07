using System;
using UnityEngine;

namespace TrekVRApplication {

    public static class TextureUtils {

        /// <summary>
        ///     Computes the expected texture size in bytes.
        /// </summary>
        /// <param name="width">Texture width in pixels. Must be power of 2.</param>
        /// <param name="height">Texture height in pixels. Must be power of 2.</param>
        /// <param name="format"></param>
        /// <param name="mimpaps">Whether the texture has mipmaps.</param>
        /// <returns></returns>
        public static long ComputeTextureSize(int width, int height,
            TextureCompressionFormat format = TextureCompressionFormat.Uncompressed, bool mimpaps = true) {

            if (format != TextureCompressionFormat.Uncompressed && format != TextureCompressionFormat.UncompressedWithAlpha) {
                throw new NotImplementedException("Only uncompressed textures are supported at this time.");
            }

            if (!MathUtils.IsPowerOfTwo(width) || !MathUtils.IsPowerOfTwo(height)) {
                throw new Exception("Image dimensions must by power of 2");
            }

            int bpp = format.BitsPerPixel();
            long size = 0;

            while (true) {
                size += width * height * bpp;

                if (width == 1 && height == 1) {
                    return size / 8;
                }

                width = MathUtils.Clamp(width >> 1, 1);
                height = MathUtils.Clamp(height >> 1, 1);

            }

        }

        public static void ComputeMipmapDimensions(int width, int height, int mipLevel, out int mipWidth, out int mipHeight) {

            if (!MathUtils.IsPowerOfTwo(width) || !MathUtils.IsPowerOfTwo(height)) {
                throw new Exception("Image dimensions must by power of 2");
            }

            width = width >> mipLevel;
            height = height >> mipLevel;

            if (width == 0 && height == 0) {
                mipWidth = 0;
                mipHeight = 0;
            }
            else {
                mipWidth = MathUtils.Clamp(width, 1);
                mipHeight = MathUtils.Clamp(height, 1);
            }

        }

        public static long ComputeMipmapSize(int width, int height, int mipLevel,
            TextureCompressionFormat format = TextureCompressionFormat.Uncompressed) {

            if (format != TextureCompressionFormat.Uncompressed && format != TextureCompressionFormat.UncompressedWithAlpha) {
                throw new NotImplementedException("Only uncompressed textures are supported at this time.");
            }

            ComputeMipmapDimensions(width, height, mipLevel, out int mipWidth, out int mipHeight);
            return mipWidth * mipHeight * format.BitsPerPixel();
        }

        public static byte[] GenerateMipmaps(RGBImage image) {
            int width = image.Width;
            int height = image.Height;
            if (!MathUtils.IsPowerOfTwo(width) || !MathUtils.IsPowerOfTwo(height)) {
                throw new Exception("Image dimensions must by power of 2");
            }
            long size = ComputeTextureSize(width, height, TextureCompressionFormat.Uncompressed);
            byte[] result = new byte[size];
            image.CopyRawData(result);
            Debug.Log($"Generated level {0} mipmap ({image.Size} bytes, width: {width}, height: {height})");
            GenerateMipmaps(image, result, image.Size, 1);
            return result;
        }

        private static void GenerateMipmaps(RGBImage image, byte[] result, long resultIndex, int level) {
            int mipWidth = MathUtils.Clamp(image.Width >> 1, 1);
            int mipHeight = MathUtils.Clamp(image.Height >> 1, 1);
            RGBImage mipImage = new RGBImage(mipWidth, mipHeight);
            for (int x = 0; x < mipWidth; x++) {
                for (int y = 0; y < mipHeight; y++) {
                    mipImage.SetPixel(x, y, image.GetAverage(x << 1, y << 1, 2, ImageBoundaryMode.Wrap));
                }
            }
            mipImage.CopyRawData(result, resultIndex);
            Debug.Log($"Generated level {level++} mipmap ({mipImage.Size} bytes, width: {mipWidth}, height: {mipHeight})");
            if (mipWidth == 1 && mipHeight == 1) {
                return;
            }
            GenerateMipmaps(mipImage, result, mipImage.Size + resultIndex, level);
        }

    }

}
