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
            $"Resolution: {info.width}x{info.height}@{info.bitsPerPixel}{info.sampleFormat}, " +
            $"Compression: {info.compression}, " +
            $"Tiled: {(info.tiled ? $"{tiff.TileRowSize()}x{tiff.TileSize() / tiff.TileRowSize()}" : "NO")}"
        );

    }

    public static TiffInfo GetInfo(Tiff tiff) {

        return new TiffInfo() {
            width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt(),
            height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt(),
            bitsPerPixel = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToShort(),
            samplesPerPixel = tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToShort(),
            sampleFormat = tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToString(),
            compression = (Compression)tiff.GetField(TiffTag.COMPRESSION)[0].ToInt(),
            tiled = tiff.IsTiled(),
        };
    }

}

// TODO Move this to its own file.
public struct TiffInfo {
    public int width;
    public int height;
    public short bitsPerPixel;
    public short samplesPerPixel;
    public string sampleFormat;
    public Compression compression;
    public bool tiled;
}
