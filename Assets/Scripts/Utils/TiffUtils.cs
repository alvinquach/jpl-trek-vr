using BitMiracle.LibTiff.Classic;
using System;
using System.IO;
using UnityEngine;

public class TiffUtils {

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

}

// TODO Move this to its own file.
public class TiffInfo {

    /// <summary> Width of the TIFF image in pixels. </summary>
    public int Width;

    /// <summary> height of the TIFF image in pixels. </summary>
    public int Height;

    /// <summary> Number of bits per pixel. </summary>
    public short BPP;

    /// <summary> Number of samples (color channels) per pixel. </summary>
    public short SPP;

    /// <summary> Sample data type (ie. float, int, etc.). </summary>
    public string SampleFormat;

    /// <summary> Compression method used by the TIFF image. </summary>
    public Compression Compression;

    /// <summary> Whether the TIFF is encoded with tiles rather than scanlines. </summary>
    public bool Tiled;
}
