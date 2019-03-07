using NUnit.Framework;
using System;
using TrekVRApplication;

namespace Tests {

    public class TextureUtilsTests {

        [Test]
        public void ComputeTextureSize_NotPowerOf2_ThrowsException() {
            Assert.Throws(typeof(Exception), () => TextureUtils.ComputeTextureSize(255, 256));
        }

        [Test]
        public void ComputeTextureSize_UncompressedA_CorrectResults() {
            long size = TextureUtils.ComputeTextureSize(256, 256, TextureCompressionFormat.Uncompressed);
            Assert.AreEqual(262143, size);
        }

        [Test]
        public void ComputeTextureSize_UncompressedWithAlphaA_CorrectResults() {
            long size = TextureUtils.ComputeTextureSize(256, 256, TextureCompressionFormat.UncompressedWithAlpha);
            Assert.AreEqual(349524, size);
        }

        [Test]
        public void ComputeMipmapDimensions_NotPowerOf2_ThrowsException() {
            Assert.Throws(typeof(Exception), () => TextureUtils.ComputeTextureSize(255, 256));
        }

        [Test]
        public void ComputeMipmapDimensions_A_CorrectResults() {
            TextureUtils.ComputeMipmapDimensions(256, 256, 0, out int mipWidth, out int mipHeight);
            Assert.AreEqual(256, mipWidth);
            Assert.AreEqual(256, mipHeight);
        }

        [Test]
        public void ComputeMipmapDimensions_B_CorrectResults() {
            TextureUtils.ComputeMipmapDimensions(256, 256, 3, out int mipWidth, out int mipHeight);
            Assert.AreEqual(32, mipWidth);
            Assert.AreEqual(32, mipHeight);
        }

        [Test]
        public void ComputeMipmapDimensions_C_CorrectResults() {
            TextureUtils.ComputeMipmapDimensions(256, 256, 9, out int mipWidth, out int mipHeight);
            Assert.AreEqual(0, mipWidth);
            Assert.AreEqual(0, mipHeight);
        }

        [Test]
        public void ComputeMipmapDimensions_D_CorrectResults() {
            TextureUtils.ComputeMipmapDimensions(256, 64, 7, out int mipWidth, out int mipHeight);
            Assert.AreEqual(2, mipWidth);
            Assert.AreEqual(1, mipHeight);
        }

        [Test]
        public void ComputeMipmapDimensions_E_CorrectResults() {
            TextureUtils.ComputeMipmapDimensions(256, 64, 8, out int mipWidth, out int mipHeight);
            Assert.AreEqual(1, mipWidth);
            Assert.AreEqual(1, mipHeight);
        }

        [Test]
        public void ComputeMipmapDimensions_F_CorrectResults() {
            TextureUtils.ComputeMipmapDimensions(256, 64, 9, out int mipWidth, out int mipHeight);
            Assert.AreEqual(0, mipWidth);
            Assert.AreEqual(0, mipHeight);
        }

        [Test]
        public void ComputeMipmapSize_NotPowerOf2_ThrowsException() {
            Assert.Throws(typeof(Exception), () => TextureUtils.ComputeTextureSize(255, 256));
        }

    }

}
