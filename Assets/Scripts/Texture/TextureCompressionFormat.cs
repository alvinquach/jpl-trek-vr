using Nvidia.TextureTools;
using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    public enum TextureCompressionFormat {
        DXT1,
        DXT5,
        Uncompressed,
        UncompressedWithAlpha
        // TODO Add more formats
    }


    public static class TextureCompressionFormatEnumExtensions {

        private static TextureCompressionFormat DefaultFormat = TextureCompressionFormat.DXT5;

        public static int BitsPerPixel(this TextureCompressionFormat format) {
            switch (format) {
                case TextureCompressionFormat.DXT1:
                    return 4;
                case TextureCompressionFormat.DXT5:
                    return 8;
                case TextureCompressionFormat.Uncompressed:
                    return 24;
                case TextureCompressionFormat.UncompressedWithAlpha:
                    return 32;
                default:
                    return default;
            }
        }

        public static TextureFormat GetUnityFormat(this TextureCompressionFormat format) {
            switch (format) {
                case TextureCompressionFormat.DXT1:
                    return TextureFormat.DXT1;
                case TextureCompressionFormat.DXT5:
                    return TextureFormat.DXT5;
                case TextureCompressionFormat.Uncompressed:
                    return TextureFormat.RGB24;
                case TextureCompressionFormat.UncompressedWithAlpha:
                    return TextureFormat.BGRA32;
                default:
                    return GetUnityFormat(DefaultFormat);
            }
        }

        public static Format GetNvttFormat(this TextureCompressionFormat format) {
            switch (format) {
                case TextureCompressionFormat.DXT1:
                    return Format.BC1;
                case TextureCompressionFormat.DXT5:
                    return Format.BC3;
                case TextureCompressionFormat.Uncompressed:
                    return Format.RGB;
                case TextureCompressionFormat.UncompressedWithAlpha:
                    return Format.RGBA;
                default:
                    return GetNvttFormat(DefaultFormat);
            }

        }

        // TODO Write functions to convert between Unity and NVTT formats.
    }
}