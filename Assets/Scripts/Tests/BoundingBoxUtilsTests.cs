using NUnit.Framework;
using TrekVRApplication;
using UnityEngine;

namespace Tests {

    public class BoundingBoxUtilsTests {

        private readonly BoundingBox _boundingBoxA = new BoundingBox(6.9f, 3.1f, 42.0f, 25.0f);
        private readonly BoundingBox _boundingBoxB = new BoundingBox(-12.3f, -45.6f, 78.9f, 0.0f);
        private readonly BoundingBox _boundingBoxC = new BoundingBox(-45.0f, 0.0f, 45.0f, 90f);
        private readonly BoundingBox _boundingBoxD = new BoundingBox(-135.0f, -90.0f, 45.0f, 0.0f);

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

        [Test]
        public void MedianLatLon_BoundingBoxA_CorrectResult() {
            Vector2 expected = new Vector2(14.05f, 24.45f);
            Vector2 actual = BoundingBoxUtils.MedianLatLon(_boundingBoxA);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MedianLatLon_BoundingBoxB_CorrectResult() {
            Vector2 expected = new Vector2(-22.8f, 33.3f);
            Vector2 actual = BoundingBoxUtils.MedianLatLon(_boundingBoxB);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MedianLatLon_BoundingBoxD_CorrectResult() {
            Vector2 expected = new Vector2(-45.0f, -45.0f);
            Vector2 actual = BoundingBoxUtils.MedianLatLon(_boundingBoxD);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MedianDirection_BoundingBoxD_CorrectResult() {
            Vector3 expected = new Vector3(0.5f, -Mathf.Sqrt(0.5f), 0.5f);
            Vector3 actual = BoundingBoxUtils.MedianDirection(_boundingBoxD);
            VectorUtils.Print(expected);
            VectorUtils.Print(actual);
            for (int i = 0; i < 3; i++) {
                CompareFloats(expected[i], actual[i]);
            }
        }

        /// <summary>
        ///     Compares two floating point precision numbers up to the given
        ///     decimal place. Asserts whether they are equal.
        /// </summary>
        /// <param name="a">The first floating point number</param>
        /// <param name="b">The second floating point number</param>
        /// <param name="precision">
        ///     (Optional) The number of decimal places to compare. Default is 4.
        /// </param>
        private void CompareFloats(float a, float b, int precision = 4) {
            if (precision > 0) {
                float multiplier = Mathf.Pow(10, precision);
                a *= multiplier;
                b *= multiplier;
            }
            Assert.AreEqual(Mathf.RoundToInt(a), Mathf.RoundToInt(b));
        }

    }
}

