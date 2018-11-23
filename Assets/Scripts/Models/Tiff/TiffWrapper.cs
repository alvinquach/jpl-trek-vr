using BitMiracle.LibTiff.Classic;
using System;
using UnityEngine;

public class TiffWrapper : IDisposable {

    private string _filePath;

    private Tiff _tiff;

    public TiffMetadata Metadata { get; private set; }

    public TiffWrapper(string filePath) {

        // Store and check the filepath.
        _filePath = filePath;
        if (string.IsNullOrEmpty(_filePath)) {
            throw new FileNotSpecifiedException("No file specified.");
        }

        // Attempt to open the TIFF image file.
        _tiff = Tiff.Open(_filePath, "r");
        if (_tiff == null) {
            throw new FileReadException($"Error reading TIFF from {filePath}");
        }

        // Extract the necessary metadata if the image was loaded sucessfully.
        GenerateMetadata();
        PrintInfo();
    }

    public IntensityImage ToIntensityImage() {

        // Check image format.
        if (Metadata.SampleFormat != SampleFormat.INT.ToString()) {
            throw new Exception("Color source cannot be converted into intensity image. Use ToRGBAImage() instead.");
        }

        // Create an Image object to store the result.
        IntensityImage result = new IntensityImage(Metadata.Width, Metadata.Height);

        // Tiled encoding...
        if (Metadata.Tiled) {

            Vector2Int tilesAcrossImage = new Vector2Int(
                Mathf.CeilToInt(Metadata.Width / (float)Metadata.TileWidth),
                Mathf.CeilToInt(Metadata.Height / (float)Metadata.TileHeight)
            );

            // Byte array for buffering the bytes read from each tile.
            byte[] tileBytes = new byte[Metadata.TileSize];

            // Float array for buffering the intensity values of each tile.
            float[] values = new float[Metadata.TileWidth * Metadata.TileHeight];

            // Iterate through each tile.
            for (int ty = 0; ty < tilesAcrossImage.y; ty++) {

                // The y-coordinate of the tile's top row of pixels on the image.
                int y = ty * Metadata.TileHeight;

                for (int tx = 0; tx < tilesAcrossImage.x; tx++) {

                    // The x-coordinate of the tile's left column of pixels on the image.
                    int x = tx * Metadata.TileWidth;

                    // Read bytes from tile and convert them to pixel values.
                    _tiff.ReadTile(tileBytes, 0, x, y, 0, 0);

                    TiffUtils.BytesToFloat(tileBytes, values);

                    // Iterate through the intensity values in the tile.
                    for (int i = 0; i < values.Length; i++) {

                        // Calculate the x and y coordinate relative to the tile
                        // based on the index of the pixel within the tile.
                        Vector2Int tilePixel = ImageUtils.IndexToCoordinates(i, Metadata.TileWidth);

                        // Update the Image object with the pixel value.
                        result.SetPixel(tilePixel.x + x, tilePixel.y + y, values[i]);
                    }
                }
            }
        }

        // Scanline encoding...
        else {

            // Byte array for buffering the bytes read from each scanline.
            byte[] scanlineBytes = new byte[Metadata.ScanlineSize];

            // Float array for buffering the intensity values of each pixels in a scanline.
            float[] values = new float[Metadata.Width];

            // Iterate through all the scanlines.
            for (int y = 0; y < Metadata.Height; y++) {

                // Read bytes from scanline and convert them to pixel values.
                _tiff.ReadScanline(scanlineBytes, y);
                TiffUtils.BytesToFloat(scanlineBytes, values);

                // Iterate through all the pixel values in the scanline.
                for (int x = 0; x < Metadata.Width; x++) {

                    // Update the Image object with the pixel value.
                    result.SetPixel(x, y, values[x]);
                }
            }
        }

        return result;
    }

    public RGBAImage ToRGBAImage() {

        // Check image format.
        if (Metadata.SampleFormat == SampleFormat.INT.ToString()) {
            throw new Exception("Grayscale source cannot be converted into color image. Use ToIntensityImage() instead.");
        }

        // Create an Image object to store the result.
        RGBAImage result = new RGBAImage(Metadata.Width, Metadata.Height);

        // Tiled encoding...
        if (Metadata.Tiled) {

            Vector2Int tilesAcrossImage = new Vector2Int(
                Mathf.CeilToInt(Metadata.Width / (float)Metadata.TileWidth),
                Mathf.CeilToInt(Metadata.Height / (float)Metadata.TileHeight)
            );

            // Byte array for buffering the bytes read from each tile.
            byte[] tileBytes = new byte[Metadata.TileSize];

            // Color32 array for buffering the intensity values of each tile.
            Color32[] values = new Color32[Metadata.TileWidth * Metadata.TileHeight];

            // Iterate through each tile.
            for (int ty = 0; ty < tilesAcrossImage.y; ty++) {

                // The y-coordinate of the tile's top row of pixels on the image.
                int y = ty * Metadata.TileHeight;

                for (int tx = 0; tx < tilesAcrossImage.x; tx++) {

                    // The x-coordinate of the tile's left column of pixels on the image.
                    int x = tx * Metadata.TileWidth;

                    // Read bytes from tile and convert them to pixel values.
                    _tiff.ReadTile(tileBytes, 0, x, y, 0, 0);

                    TiffUtils.BytesToColor32(tileBytes, values);

                    // Iterate through the intensity values in the tile.
                    for (int i = 0; i < values.Length; i++) {

                        // Calculate the x and y coordinate relative to the tile
                        // based on the index of the pixel within the tile.
                        Vector2Int tilePixel = ImageUtils.IndexToCoordinates(i, Metadata.TileWidth);

                        // Update the Image object with the pixel value.
                        result.SetPixel(tilePixel.x + x, tilePixel.y + y, values[i]);
                    }
                }
            }
        }

        // Scanline encoding...
        else {

            // Byte array for buffering the bytes read from each scanline.
            byte[] scanlineBytes = new byte[Metadata.ScanlineSize];

            // Color32 array for buffering the intensity values of each pixels in a scanline.
            Color32[] values = new Color32[Metadata.Width];

            // Iterate through all the scanlines.
            for (int y = 0; y < Metadata.Height; y++) {

                // Read bytes from scanline and convert them to pixel values.
                _tiff.ReadScanline(scanlineBytes, y);
                TiffUtils.BytesToColor32(scanlineBytes, values);

                // Iterate through all the pixel values in the scanline.
                for (int x = 0; x < Metadata.Width; x++) {

                    // Update the Image object with the pixel value.
                    result.SetPixel(x, y, values[x]);
                }
            }
        }

        return result;
    }

    public void PrintInfo(string header = "TIFF Info:") {
        Debug.Log(
            $"{header}\n" +
            $"Resolution: {Metadata.Width}x{Metadata.Height}@{Metadata.BPP}{Metadata.SampleFormat}, " +
            $"Compression: {Metadata.Compression}, " +
            $"Tiled: {(Metadata.Tiled ? $"{_tiff.TileRowSize()}x{Metadata.TileSize / _tiff.TileRowSize()}" : "NO")}"
        );
    }

    public void Dispose() {
        _tiff.Dispose();
    }

    private void GenerateMetadata() {
        if (_tiff == null) {
            return;
        }

        bool tiled = _tiff.IsTiled();

        Metadata = new TiffMetadata() {
            Width = _tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt(),
            Height = _tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt(),
            BPP = _tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToShort(),
            SPP = _tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToShort(),
            SampleFormat = _tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToString(),
            Compression = (Compression)_tiff.GetField(TiffTag.COMPRESSION)[0].ToInt(),
            Tiled = tiled,
            TileSize = tiled ? _tiff.TileSize() : 0,
            TileWidth = tiled ? _tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt() : 0,
            TileHeight = tiled ? _tiff.GetField(TiffTag.TILELENGTH)[0].ToInt() : 0,
            ScanlineSize = tiled ? 0 : _tiff.ScanlineSize()
        };
    }

    public static bool operator true(TiffWrapper o) {
        return o != null;
    }

    public static bool operator false(TiffWrapper o) {
        return o == null;
    }

    public static bool operator !(TiffWrapper o) {
        return o ? false : true;
    }

}