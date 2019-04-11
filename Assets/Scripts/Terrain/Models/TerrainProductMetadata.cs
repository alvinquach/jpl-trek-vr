using System;
using System.Text;

namespace TrekVRApplication {

    public struct TerrainProductMetadata : IEquatable<TerrainProductMetadata> {

        public string ProductUUID { get; }

        public IBoundingBox BoundingBox { get; }

        public int Width { get; }

        public int Height { get; }

        public ImageFileFormat Format { get; set; }

        public TerrainProductMetadata(string productUUID, IBoundingBox boundingBox, int size, ImageFileFormat format = 0) :
            this(productUUID, boundingBox, size, size, format) {

        }

        public TerrainProductMetadata(string productUUID, IBoundingBox boundingBox, int width, int height, ImageFileFormat format = 0) {
            ProductUUID = productUUID;
            BoundingBox = boundingBox;
            Width = width;
            Height = height;
            Format = format;
        }

        /// <summary>
        ///     Encodes the metadata into a base64 string that can be used as a filename.
        /// </summary>
        public string EncodeBase64() {
            string rawFilename = ToString("|");
            byte[] bytes = Encoding.UTF8.GetBytes(rawFilename);
            return Convert.ToBase64String(bytes);
        }

        public static TerrainProductMetadata DecodeBase64(string filename) {
            // TODO Implement this
            throw new NotImplementedException();
        }

        public string ToString(string delimiter) {
            return ProductUUID + delimiter +
                BoundingBox.ToString(",") + delimiter +
                $"{Width}x{Height}" + delimiter +
                Format.ToString();
        }

        public override string ToString() {
            return ToString("\n");
        }

        public bool Equals(TerrainProductMetadata other) {
            return ProductUUID == other.ProductUUID &&
                   BoundingBox == other.BoundingBox &&
                   Width == other.Width &&
                   Height == other.Height &&
                   Format == other.Format;
        }

    }

}