using BitMiracle.LibTiff.Classic;
using System;
using UnityEngine;

namespace TrekVRApplication
{

    public static class TiffImageConverter {

        public static IntensityImage ToIntensityImage(TiffImage tiff) {
            TiffMetadata metadata = tiff.Metadata;

            // Check image format.
            if (metadata.SampleFormat != SampleFormat.INT.ToString()) {
                throw new Exception("Color source cannot be converted into intensity image. Use ToRGBAImage() instead.");
            }

            // Create an Image object to store the result.
            IntensityImage result = new IntensityImage(metadata.Width, metadata.Height);

            // Tiled encoding...
            if (metadata.Tiled) {

                Vector2Int tilesAcrossImage = new Vector2Int(
                    Mathf.CeilToInt(metadata.Width / (float)metadata.TileWidth),
                    Mathf.CeilToInt(metadata.Height / (float)metadata.TileHeight)
                );

                // Byte array for buffering the bytes read from each tile.
                byte[] tileBytes = new byte[metadata.TileSize];

                // Float array for buffering the intensity values of each tile.
                float[] values = new float[metadata.TileWidth * metadata.TileHeight];

                // Iterate through each tile.
                for (int ty = 0; ty < tilesAcrossImage.y; ty++) {

                    // The y-coordinate of the tile's top row of pixels on the image.
                    int y = ty * metadata.TileHeight;

                    for (int tx = 0; tx < tilesAcrossImage.x; tx++) {

                        // The x-coordinate of the tile's left column of pixels on the image.
                        int x = tx * metadata.TileWidth;

                        // Read bytes from tile and convert them to pixel values.
                        tiff.ReadTile(tileBytes, 0, x, y, 0, 0);

                        TiffUtils.BytesToFloat(tileBytes, values);

                        // Iterate through the intensity values in the tile.
                        for (int i = 0; i < values.Length; i++) {

                            // Calculate the x and y coordinate relative to the tile
                            // based on the index of the pixel within the tile.
                            Vector2Int tilePixel = ImageUtils.IndexToCoordinates(i, metadata.TileWidth);

                            // Update the Image object with the pixel value.
                            result.SetPixel(tilePixel.x + x, tilePixel.y + y, values[i]);
                        }
                    }
                }
            }

            // Scanline encoding...
            else {

                // Byte array for buffering the bytes read from each scanline.
                byte[] scanlineBytes = new byte[metadata.ScanlineSize];

                // Float array for buffering the intensity values of each pixels in a scanline.
                float[] values = new float[metadata.Width];

                // Iterate through all the scanlines.
                for (int y = 0; y < metadata.Height; y++) {

                    // Read bytes from scanline and convert them to pixel values.
                    tiff.ReadScanline(scanlineBytes, y);
                    TiffUtils.BytesToFloat(scanlineBytes, values);

                    // Iterate through all the pixel values in the scanline.
                    for (int x = 0; x < metadata.Width; x++) {

                        // Update the Image object with the pixel value.
                        result.SetPixel(x, y, values[x]);
                    }
                }
            }

            return result;
        }

        public static T ToColorImage<T>(TiffImage tiff) where T : ColorImage {
            TiffMetadata metadata = tiff.Metadata;

            // Check image format.
            if (metadata.SampleFormat == SampleFormat.INT.ToString()) {
                // TODO Add ability to convert grayscale to color image.
                throw new Exception("Grayscale source cannot be converted into color image. Use ToIntensityImage() instead.");
            }

            // Create an Image object to store the result.
            T result = (T)Activator.CreateInstance(typeof(T), metadata.Width, metadata.Height);

            // Tiled encoding...
            if (metadata.Tiled) {

                Vector2Int tilesAcrossImage = new Vector2Int(
                    Mathf.CeilToInt(metadata.Width / (float)metadata.TileWidth),
                    Mathf.CeilToInt(metadata.Height / (float)metadata.TileHeight)
                );

                // Byte array for buffering the bytes read from each tile.
                byte[] tileBytes = new byte[metadata.TileSize];

                // Color32 array for buffering the intensity values of each tile.
                Color32[] values = new Color32[metadata.TileWidth * metadata.TileHeight];

                // Iterate through each tile.
                for (int ty = 0; ty < tilesAcrossImage.y; ty++) {

                    // The y-coordinate of the tile's top row of pixels on the image.
                    int y = ty * metadata.TileHeight;

                    for (int tx = 0; tx < tilesAcrossImage.x; tx++) {

                        // The x-coordinate of the tile's left column of pixels on the image.
                        int x = tx * metadata.TileWidth;

                        // Read bytes from tile and convert them to pixel values.
                        tiff.ReadTile(tileBytes, 0, x, y, 0, 0);

                        TiffUtils.BytesToColor32(tileBytes, values);

                        // Iterate through the intensity values in the tile.
                        for (int i = 0; i < values.Length; i++) {

                            // Calculate the x and y coordinate relative to the tile
                            // based on the index of the pixel within the tile.
                            Vector2Int tilePixel = ImageUtils.IndexToCoordinates(i, metadata.TileWidth);

                            // Update the Image object with the pixel value.
                            result.SetPixel(tilePixel.x + x, tilePixel.y + y, values[i]);
                        }
                    }
                }
            }

            // Scanline encoding...
            else {

                // Byte array for buffering the bytes read from each scanline.
                byte[] scanlineBytes = new byte[metadata.ScanlineSize];

                // Color32 array for buffering the intensity values of each pixels in a scanline.
                Color32[] values = new Color32[metadata.Width];

                // Iterate through all the scanlines.
                for (int y = 0; y < metadata.Height; y++) {

                    // Read bytes from scanline and convert them to pixel values.
                    tiff.ReadScanline(scanlineBytes, y);
                    TiffUtils.BytesToColor32(scanlineBytes, values);

                    // Iterate through all the pixel values in the scanline.
                    for (int x = 0; x < metadata.Width; x++) {

                        // Update the Image object with the pixel value.
                        result.SetPixel(x, y, values[x]);
                    }
                }
            }

            return result;
        }

    }

}
