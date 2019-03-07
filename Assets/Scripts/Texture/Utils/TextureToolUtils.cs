using Nvidia.TextureTools;
using System;
using System.Runtime.InteropServices;

namespace TrekVRApplication {

    /// <summary>
    ///     Helper class for working with Nvidia Texture Tools (NVTT).
    /// </summary>
    [Obsolete("nVidia Texture Tools is no longer used.")]
    public static class TextureToolUtils {

        private class BytesWrapper {
            public byte[] Bytes;
        }

        /// <summary>
        ///     Converts an RGBAImage object into a byte array that can be loaded into a texture.
        /// </summary>
        /// <param name="image">The Imgae object to convert.</param>
        /// <param name="format">The resulting texture format/compression.</param>
        /// <param name="mipmaps">Whether to generate mipmaps for the texture.</param>
        /// <returns>
        ///     A byte array that can be loaded into a Unity Texture2D.
        /// </returns>
        public static byte[] ImageToTexture(RGBAImage image, TextureCompressionFormat format = TextureCompressionFormat.DXT5, bool mipmaps = true) {

            // Retrieve the raw pixel data from the image.
            byte[] srcBytes = new byte[image.Size];
            image.CopyRawData(srcBytes);

            /* 
             * Allocate a handle for the pixel data so that it doesn't get garbage collected.
             * This will allow a the pixel array to be reprsented as an pointer so that it
             * can be used by the Nvidia Texture Tools.
             */
            GCHandle pinnedArray = GCHandle.Alloc(srcBytes, GCHandleType.Pinned);
            IntPtr srcBytesPtr = pinnedArray.AddrOfPinnedObject();

            /*
             * Initialize an array to store the generated data with a size of zero. The array
             * will be resized as needed. A wrapper is used so that the array can be resized
             * within the lambda callback expression in the OutputOptions.
             */
            BytesWrapper destBytes = new BytesWrapper() { Bytes = new byte[0] };

            InputOptions inputOptions = GenerateInputOptions(srcBytesPtr, image.Width, image.Height, mipmaps);
            CompressionOptions compressionOptions = GenerateCompressionOptions(format);
            OutputOptions outputOptions = GenerateOutputOptions(destBytes);

            Compressor compressor = new Compressor();
            compressor.Compress(inputOptions, compressionOptions, outputOptions);

            // Free the allocated handle so that it be garbage collected.
            pinnedArray.Free();

            return destBytes.Bytes;
        }

        /// <summary>
        ///     Helper method to generate an InputOptions object for use with Nvidia Texture Tools.
        /// </summary>
        private static InputOptions GenerateInputOptions(IntPtr srcBytes, int width, int height, bool mipmaps = true) {
            InputOptions inputOptions = new InputOptions();
            inputOptions.SetTextureLayout(TextureType.Texture2D, width, height, 1);
            inputOptions.SetMipmapData(srcBytes, width, height, 1, 0, 0);
            inputOptions.SetMipmapGeneration(mipmaps);
            if (mipmaps) {
                inputOptions.SetMipmapFilter(MipmapFilter.Box);
            }
            inputOptions.SetAlphaMode(AlphaMode.None); // TODO Add a param to set this.
            return inputOptions;
        }

        /// <summary>
        ///     Helper method to generate an CompressionOptions object for use with Nvidia Texture Tools.
        /// </summary>
        private static CompressionOptions GenerateCompressionOptions(TextureCompressionFormat format) {
            CompressionOptions compressionOptions = new CompressionOptions();
            compressionOptions.SetFormat(format.GetNvttFormat());
            return compressionOptions;
        }

        /// <summary>
        ///     Helper method to generate an OutputOptions object for use with Nvidia Texture Tools.
        /// </summary>
        private static OutputOptions GenerateOutputOptions(BytesWrapper destBytes) {
            int mipIndex = -1, destIndex = 0;
            OutputOptions outputOptions = new OutputOptions();
            outputOptions.SetOutputOptionsOutputHandler(

                // BeginImageHandler. This is called for each mip level.
                (size, width, height, depth, face, miplevel) => {
                    mipIndex = miplevel; // Set the current mipmap level.
                },

                // OutputHandler. This is called for header data and for each mip level.
                (data, size) => {
                    if (mipIndex == -1) {
                        // Header data, do nothing.
                    }
                    else {

                        // Resize the destination array if needed.
                        int currentSize = destBytes.Bytes.Length;
                        if (currentSize - destIndex < size) {
                            Array.Resize(ref destBytes.Bytes, destIndex + size);
                        }

                        // Copy the generated mip data into the destination array.
                        Marshal.Copy(data, destBytes.Bytes, destIndex, size);

                        destIndex += size;
                    }
                    return true;
                },

                // EndImageHandler
                () => {
                    // Do nothing.
                }
            );
            return outputOptions;
        }

    }

}