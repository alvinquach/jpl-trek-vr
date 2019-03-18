using BitMiracle.LibTiff.Classic;
using System;
using System.IO;
using UnityEngine;

namespace TrekVRApplication {

    public sealed class TiffUtils {

        private TiffUtils() { }

        #region Tiff Object Constructors

        [Obsolete]
        public static Tiff FromFilePath(string filePath) {

            if (String.IsNullOrEmpty(filePath)) {
                throw new FileNotSpecifiedException("No file specified.");
            }

            Tiff tiff = Tiff.Open(filePath, "r");

            if (tiff == null) {
                throw new FileReadException("Error reading TIFF from " + filePath);
            }

            return tiff;
        }

        [Obsolete]
        public static Tiff FromBytes(byte[] bytes) {

            Tiff tiff = Tiff.ClientOpen("in-memory", "r", new MemoryStream(bytes), new TiffStream());

            if (tiff == null) {
                throw new FileReadException("Error reading TIFF from memory");
            }

            return tiff;
        }

        #endregion

        #region Array Operation Methods

        // TODO Write methods to shift values in an array along the x and y directions.

        #endregion

        #region Bit Conversion Methods

        /// <summary>
        ///     Takes a byte array of a tile containing intensity values in either single-precision floats or
        ///     signed shorts, converts the bytes into single-precision floating point numbers, and then
        ///     writes it to the provided buffer.
        /// </summary>
        /// <param name="bytes">The byte array of a tile containg single-precision floating point numbers.</param>
        /// <param name="buf">The buffer to write to.</param>
        /// <param name="tileWidth">The expected witdh of the tile in pixels (or number of float values).</param>
        /// <param name="offset">The start pixel of the buffer where the tile will be written to.</param>
        public static void IntensityTileToFloat(byte[] bytes, TiffSampleFormat format, float[,] buf, int tileWidth, Vector2Int offset) {

            int bytesPerSample = format.BitsPerSample / 8;

            if (bytes.Length % bytesPerSample != 0) {
                throw new Exception($"Length of byte array must be multiple of {format.BitsPerSample}");
            }

            // The total number of float values in each tile.
            int tileLength = bytes.Length / bytesPerSample;

            // The data length of each tile must be evenly divisible by the tile width.
            if (tileLength % tileWidth != 0) {
                throw new Exception($"Wrong tile size of {bytes.Length} for tileWidth={tileWidth}");
            }

            int tileHeight = tileLength / tileWidth;

            for (int dy = 0; dy < tileHeight; dy++) {
                int y = offset.y + dy;
                if (y >= buf.GetLength(1)) {
                    break;
                }
                int i = dy * tileWidth;
                for (int dx = 0; dx < tileWidth; dx++) {
                    int x = offset.x + dx;
                    if (x >= buf.GetLength(0)) {
                        break;
                    }
                    buf[x, y] = format.ConvertBits(bytes, bytesPerSample * i++);
                }
            }

        }

        /// <summary>
        ///     Invokes either SingleToFloat() or Int16ToFloat() depending on the number of bits per pixel.
        ///     The bits per pixel is automatically calculated from the buffer size and byte array size.
        /// </summary>
        /// <param name="bytes">The byte array to be converted to floating point values.</param>
        /// <param name="buf">The array to store the converted floating point numbers.</param>
        public static void BytesToFloat(byte[] bytes, float[] buf) {
            if (bytes.Length % buf.Length != 0) {
                // TODO Throw exception
                return;
            }
            int bpp = (bytes.Length / buf.Length) << 3;
            if (bpp == 32) {
                SingleToFloat(bytes, buf);
            }
            else {
                Int16ToFloat(bytes, buf);
            }
        }

        /// <summary>
        ///     Takes an array of bytes where every four consecutive bytes represents a single-precision 
        ///     floating point number, and outputs the single-precision floating point numbers to an array.
        /// </summary>
        /// <param name="bytes">The bytes of an array of single-precision floating point numbers.</param>
        /// <param name="buf">The array to store the converted floating point numbers.</param>
        public static void SingleToFloat(byte[] bytes, float[] buf) {
            if (bytes.Length % 4 != 0) {
                // TODO Throw exception
                return;
            }
            int length = bytes.Length / 4;
            if (buf.Length != length) {
                // TODO Throw exception
                return;
            }
            for (int i = 0; i < length; i++) {
                // TODO Check if CPU uses little endian.
                buf[i] = BitConverter.ToSingle(bytes, 4 * i);
            }
        }

        /// <summary>
        ///     Takes an array of bytes where every two consecutive bytes represents a signed integer,
        ///     and outputs the single-precision floating point numbers to an array.
        /// </summary>
        /// <param name="bytes">The bytes of an array of signed integers.</param>
        /// <param name="buf">The array to store the converted floating point numbers.</param>
        public static void Int16ToFloat(byte[] bytes, float[] buf) {
            if (bytes.Length % 2 != 0) {
                // TODO Throw exception
                return;
            }
            int length = bytes.Length / 2;
            if (buf.Length != length) {
                // TODO Throw exception
                return;
            }
            for (int i = 0; i < length; i++) {
                // TODO Check if CPU uses little endian.
                buf[i] = BitConverter.ToInt16(bytes, 2 * i);
            }
        }

        /// <summary>
        ///     Takes an array of bytes containing contiguous RGB byte samples and separates it into 3 arrays,
        ///     each containing samples from a single channel.
        /// </summary>
        [Obsolete]
        public static byte[,] SeparateContiguousSamples(byte[] bytes) {
            if (bytes.Length % 3 != 0) {
                return null; // TODO Throw exception
            }
            int length = bytes.Length / 3;
            byte[,] result = new byte[3, length];
            for (int i = 0; i < bytes.Length; i += 3) {
                for (int j = 0; j < 3; j++) {
                    result[j, i] = bytes[i + j];
                }
            }
            return result;
        }

        /// <summary>
        ///     Takes an array of bytes representing RGB pixels and outputs the data into a Color32 array.
        /// </summary>
        public static void BytesToColor32(byte[] bytes, Color32[] buf) {
            if (bytes.Length % 3 != 0) {
                // TODO Throw exception
                return;
            }
            int length = bytes.Length / 3;
            if (buf.Length != length) {
                // TODO Throw exception
                return;
            }
            for (int i = 0; i < length; i++) {
                int j = i * 3;
                Color32 color = new Color32(bytes[j], bytes[j + 1], bytes[j + 2], 255);
                buf[i] = color;
            }
        }

        #endregion

    }

}