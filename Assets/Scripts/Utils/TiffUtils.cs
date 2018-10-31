using BitMiracle.LibTiff.Classic;
using System;
using System.IO;
using UnityEngine;

public sealed class TiffUtils {

    private TiffUtils() { }

    #region Tiff Object Constructors

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
    ///     Takes an array of bytes where every four consecutive bytes represents a single-precision 
    ///     floating point number, and outputs an array of single-precision floating point numbers.
    /// </summary>
    /// <param name="bytes">The bytes of an array of single-precision floating point numbers.</param>
    /// <returns>An array of single-precision floating point numbers.</returns>
    [Obsolete]
    public static float[] Array32ToFloat(byte[] bytes) {
        if (bytes.Length % 4 != 0) {
            return null;
        }
        int length = bytes.Length / 4;
        float[] result = new float[length];
        for (int i = 0; i < length; i++) {
            // TODO Check if CPU uses little endian.
            result[i] = BitConverter.ToSingle(bytes, 4 * i);
        }
        return result;
    }

    /// <summary>
    ///     Takes an array of bytes where every two consecutive bytes represents a signed integer,
    ///     and outputs an array of single-precision floating point numbers.
    /// </summary>
    /// <param name="bytes">The bytes of an array of signed integers.</param>
    /// <returns>An array of single-precision floating point numbers.</returns>
    [Obsolete]
    public static float[] Array16ToFloat(byte[] bytes) {
        if (bytes.Length % 2 != 0) {
            return null;
        }
        int length = bytes.Length / 2;
        float[] result = new float[length];
        for (int i = 0; i < length; i++) {
            // TODO Check if CPU uses little endian.
            result[i] = BitConverter.ToInt16(bytes, 2 * i);
        }
        return result;
    }

    /// <summary>
    ///     Takes an array of bytes containing contiguous RGB byte samples and separates it into 3 arrays,
    ///     each containing samples from a single channel.
    /// </summary>
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

    #endregion

    #region Metadata Methods

    public static void PrintInfo(Tiff tiff, string header = "TIFF Info:") {

        TiffInfo info = GetInfo(tiff);

        Debug.Log(
            $"{header}\n" +
            $"Resolution: {info.Width}x{info.Height}@{info.BPP}{info.SampleFormat}, " +
            $"Compression: {info.Compression}, " +
            $"Tiled: {(info.Tiled ? $"{tiff.TileRowSize()}x{tiff.TileSize() / tiff.TileRowSize()}" : "NO")}"
        );

    }

    public static TiffInfo GetInfo(Tiff tiff) {

        if (tiff == null) {
            return null;
        }

        return new TiffInfo() {
            Width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt(),
            Height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt(),
            BPP = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToShort(),
            SPP = tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToShort(),
            SampleFormat = tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToString(),
            Compression = (Compression)tiff.GetField(TiffTag.COMPRESSION)[0].ToInt(),
            Tiled = tiff.IsTiled(),
        };
    }

    #endregion

}