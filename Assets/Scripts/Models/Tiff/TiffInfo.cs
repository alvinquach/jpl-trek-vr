using BitMiracle.LibTiff.Classic;

/// <summary>
///     A data transfer object containing the information about a TIFF image.
/// </summary>
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