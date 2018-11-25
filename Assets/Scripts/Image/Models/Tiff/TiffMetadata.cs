using BitMiracle.LibTiff.Classic;

/// <summary>
///     A data transfer object containing the information about a TIFF image.
/// </summary>
public class TiffMetadata {

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

    /// <summary> Tile size in bytes. </summary>
    public int TileSize;

    /// <summary> Tile width in pixels. </summary>
    public int TileWidth;

    /// <summary> Tile height in pixels. </summary>
    public int TileHeight;

    /// <summary> Scanline size in bytes. </summary>
    public int ScanlineSize;

    public static bool operator true(TiffMetadata o) {
        return o != null;
    }

    public static bool operator false(TiffMetadata o) {
        return o == null;
    }

    public static bool operator !(TiffMetadata o) {
        return o ? false : true;
    }

}