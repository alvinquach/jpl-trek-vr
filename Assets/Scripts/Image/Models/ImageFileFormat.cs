namespace TrekVRApplication {

    public enum ImageFileFormat : int {

        Unknown = 0,
        Tiff = 1,
        Png = 2,
        Jpeg = 3

    }

    public static class ImageFileFormatEnumExtensions {

        public static string FileExtension(this ImageFileFormat format) {
            switch (format) {
                case ImageFileFormat.Tiff:
                    return "tiff";
                case ImageFileFormat.Png:
                    return "png";
                case ImageFileFormat.Jpeg:
                    return "jpg";
                default:
                    return "";
            }

        }

    }
}