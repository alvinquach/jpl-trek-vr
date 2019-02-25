using Nvidia.TextureTools;
using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    public enum TextureCompressionFormat {
        DXT1,
        DXT5
        // TODO Add more formats
    }


    public static class TextureCompressionFormatEnumExtensions {

        private static TextureCompressionFormat DefaultFormat = TextureCompressionFormat.DXT5;

        // TODO Use switch cases instead of maps.

        private static readonly IDictionary<TextureCompressionFormat, TextureFormat> UnityFormatMap = 
            new Dictionary<TextureCompressionFormat, TextureFormat>() {
                { TextureCompressionFormat.DXT1, TextureFormat.DXT1 },
                { TextureCompressionFormat.DXT5, TextureFormat.DXT5 }
            };

        private static readonly IDictionary<TextureCompressionFormat, Format> NvttFormatMap =
            new Dictionary<TextureCompressionFormat, Format>() {
                { TextureCompressionFormat.DXT1, Format.BC1 },
                { TextureCompressionFormat.DXT5, Format.BC3 }
            };

        public static TextureFormat GetUnityFormat(this TextureCompressionFormat format) {
            if (!UnityFormatMap.ContainsKey(format)) {
                format = DefaultFormat;
            }
            return UnityFormatMap[format];
        }

        public static Format GetNvttFormat(this TextureCompressionFormat format) {
            if (!NvttFormatMap.ContainsKey(format)) {
                format = DefaultFormat;
            }
            return NvttFormatMap[format];
        }

    }

    // TODO Write functions to convert between Unity and NVTT formats.

}