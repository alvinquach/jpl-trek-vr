using System;
using System.Text;

namespace TrekVRApplication {

    public struct TerrainModelProductMetadata : IEquatable<TerrainModelProductMetadata> {

        public string ProductId { get; }

        public BoundingBox BoundingBox { get; }

        public int Width { get; }

        public int Height { get; }

        public ImageFileFormat Format { get; set; }

        public TerrainModelProductMetadata(string productId, BoundingBox boundingBox, int size, ImageFileFormat format = 0) :
            this(productId, boundingBox, size, size, format) {

        }

        public TerrainModelProductMetadata(string productId, BoundingBox boundingBox, int width, int height, ImageFileFormat format = 0) {
            ProductId = productId;
            BoundingBox = boundingBox;
            Width = width;
            Height = height;
            Format = format;
        }

        /// <summary>
        ///     Encodes the metadata into a base64 string that can be used as a filename.
        /// </summary>
        public string EncodeBase64() {
            string rawFilename = $"{ProductId}|{BoundingBox.ToString(",")}|{Width}x{Height}|{Format}";
            byte[] bytes = Encoding.UTF8.GetBytes(rawFilename);
            return Convert.ToBase64String(bytes);
        }

        public static TerrainModelProductMetadata DecodeBase64(string filename) {
            // TODO Implement this
            throw new NotImplementedException();
        }

        public bool Equals(TerrainModelProductMetadata other) {
            return ProductId == other.ProductId &&
                   BoundingBox == other.BoundingBox &&
                   Width == other.Width &&
                   Height == other.Height &&
                   Format == other.Format;
        }

    }

}