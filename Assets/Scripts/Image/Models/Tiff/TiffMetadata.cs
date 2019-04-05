using BitMiracle.LibTiff.Classic;

namespace TrekVRApplication {

    /// <summary>
    ///     A data transfer object containing the information about a TIFF image.
    /// </summary>
    public struct TiffMetadata {

        /// <summary> Width of the TIFF image in pixels. </summary>
        public int Width { get; }

        /// <summary> height of the TIFF image in pixels. </summary>
        public int Height { get; }

        /// <summary> Number of bits per color sample. </summary>
        public short BitsPerSample { get; }

        /// <summary> Number of color samples (channels) per pixel. </summary>
        public short SamplesPerPixel { get; }

        /// <summary> Sample data type (ie. float; int; etc.). </summary>
        public string SampleFormat { get; }

        /// <summary> Compression method used by the TIFF image. </summary>
        public Compression Compression { get; }

        /// <summary> Whether the TIFF is encoded with tiles rather than scanlines. </summary>
        public bool Tiled { get; }

        /// <summary> Tile size in bytes. </summary>
        public int TileSize { get; }

        /// <summary> Tile width in pixels. </summary>
        public int TileWidth { get; }

        /// <summary> Tile height in pixels. </summary>
        public int TileHeight { get; }

        /// <summary> Scanline size in bytes. </summary>
        public int ScanlineSize { get; }

        public TiffMetadata(Tiff tiff) {
            if (tiff == null) {
                // TODO Throw exception
            }

            Width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            Height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            BitsPerSample = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToShort();
            SamplesPerPixel = tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToShort();
            
            // FIXME Handle null field properly (field can be null when TIFF is saved from Photoshop).
            SampleFormat = tiff.GetField(TiffTag.SAMPLEFORMAT)?[0].ToString();

            Compression = (Compression)tiff.GetField(TiffTag.COMPRESSION)[0].ToInt();
            Tiled = tiff.IsTiled();
            TileSize = Tiled ? tiff.TileSize() : 0;
            TileWidth = Tiled ? tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt() : 0;
            TileHeight = Tiled ? tiff.GetField(TiffTag.TILELENGTH)[0].ToInt() : 0;
            ScanlineSize = Tiled ? 0 : tiff.ScanlineSize();
        }

    }

}
