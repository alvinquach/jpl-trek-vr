using System;
using System.Text;

namespace TrekVRApplication {

    public struct TerrainModelFileMetadata {

        // TODO Turn this into an enum
        public int product;

        public BoundingBox boundingBox;

        public int size;

        // TODO Turn this into an enum
        public string format;

        public TerrainModelFileMetadata(int product, BoundingBox boundingBox, int size, string format) {
            this.product = product;
            this.boundingBox = boundingBox;
            this.size = size;
            this.format = format;
        }

        /// <summary>
        ///     Encodes the metadata into a base64 string that can be used as a filename.
        /// </summary>
        public string EncodeBase64() {
            string rawFilename = $"{product} {boundingBox.ToString(",")} {size} {format}";
            byte[] bytes = Encoding.UTF8.GetBytes(rawFilename);
            return Convert.ToBase64String(bytes);
        }

        public static TerrainModelFileMetadata DecodeBase64(string filename) {
            // TODO Implement this
            throw new NotImplementedException();
        }

    }

}