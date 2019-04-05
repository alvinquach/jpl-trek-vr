using BitMiracle.LibTiff.Classic;
using System;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Wraps the Tiff class from BitMiracle.LibTiff.Classic and
    ///     provide additional functionality.
    /// </summary>
    public class TiffImage : IDisposable {

        private readonly string _filePath;

        private readonly Tiff _tiff;

        public TiffMetadata Metadata { get; }

        public TiffImage(string filePath) {

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
            Metadata = new TiffMetadata(_tiff);
            PrintInfo();
        }

        #region BitMiracle.LibTiff.Classic.Tiff method wrappers

        /// <summary>
        ///     Wraps the Tiff.ReadTile method.
        /// </summary>
        public int ReadTile(byte[] buffer, int offset, int x, int y, int z, short plane) {
            return _tiff.ReadTile(buffer, offset, x, y, z, plane);
        }

        /// <summary>
        ///     Wraps the Tiff.ReadScanline method.
        /// </summary>
        public bool ReadScanline(byte[] buffer, int row) {
            return _tiff.ReadScanline(buffer, row);
        }

        #endregion

        public void PrintInfo(string header = "TIFF Info:") {
            Debug.Log(
                $"{header}\n" +
                $"Resolution: {Metadata.Width}x{Metadata.Height}@{Metadata.BitsPerSample}bits*{Metadata.SamplesPerPixel}spp, " +
                $"Sample Format: {Metadata.SampleFormat}, " +
                $"Compression: {Metadata.Compression}, " +
                $"Tiled: {(Metadata.Tiled ? $"{_tiff.TileRowSize()}x{Metadata.TileSize / _tiff.TileRowSize()}" : "NO")}"
            );
        }

        public void Dispose() {
            _tiff.Dispose();
        }

        public static bool operator true(TiffImage o) {
            return o != null;
        }

        public static bool operator false(TiffImage o) {
            return o == null;
        }

        public static bool operator !(TiffImage o) {
            return o ? false : true;
        }

    }

}