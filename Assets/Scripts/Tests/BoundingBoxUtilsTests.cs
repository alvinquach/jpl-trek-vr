using NUnit.Framework;
using TrekVRApplication;

namespace Tests {

    public class BoundingBoxUtilsTests {

        private readonly BoundingBox _boundingBoxA = new BoundingBox(6.9f, 3.1f, 42.0f, 25.0f);
        private readonly BoundingBox _boundingBoxB = new BoundingBox(-12.3f, -45.6f, 78.9f, 0.0f);
        private readonly BoundingBox _boundingBoxC = new BoundingBox(-45.0f, 0.0f, 45.0f, 90f);

        [Test]
        public void ParseBoundingBox_BoundingBoxA_ParsesCorrectly() {
            string boundingBoxString = "6.90,3.10,42.00,25.00";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(boundingBox, _boundingBoxA);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxAReversed_ParsesCorrectly() {
            string boundingBoxString = "42.00,25.00,6.90,3.10";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(boundingBox, _boundingBoxA);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxB_ParsesCorrectly() {
            string boundingBoxString = "-12.3,-45.6,78.9,0";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(boundingBox, _boundingBoxB);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxBReversed_ParsesCorrectly() {
            string boundingBoxString = "78.9,0,-12.3,-45.6";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(boundingBox, _boundingBoxB);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxCLatWrapped_ParsesCorrectly() {
            string boundingBoxString = "-45,0,45,99";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(boundingBox, _boundingBoxC);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxCLongWrapped_ParsesCorrectly() {
            string boundingBoxString = "315,0,45,90";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(boundingBox, _boundingBoxC);
        }

    }
}
