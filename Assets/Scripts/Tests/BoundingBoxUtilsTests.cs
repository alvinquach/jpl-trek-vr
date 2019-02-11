using NUnit.Framework;
using System;
using TrekVRApplication;
using UnityEngine;

namespace Tests {

    public class BoundingBoxUtilsTests {

        private readonly int _floatCompareDecimalPrecision = 4;

        private readonly BoundingBox _boundingBoxA = new BoundingBox(6.9f, 3.1f, 42.0f, 25.0f);
        private readonly BoundingBox _boundingBoxB = new BoundingBox(-12.3f, -45.6f, 78.9f, 0.0f);
        private readonly BoundingBox _boundingBoxC = new BoundingBox(-45.0f, 0.0f, 45.0f, 90f);
        private readonly BoundingBox _boundingBoxD = new BoundingBox(-135.0f, -90.0f, 45.0f, 0.0f);
        private readonly BoundingBox _boundingBoxE = new BoundingBox(-20.0f, -0.0f, 10.0f, 90.0f);

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
            for (int i = 0; i < 3; i++) {
                CompareFloats(expected[i], actual[i]);
            }
        }

        [Test]
        public void ExpandToSquare_BoundingBoxACenter_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, -3.5f, 42.0f, 31.6f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.Center);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxALeft_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, -3.5f, 42.0f, 31.6f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.Left);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxARight_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, -3.5f, 42.0f, 31.6f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.Right);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxATop_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, -10.1f, 42.0f, 25.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.Top);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxATopLeft_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, -10.1f, 42.0f, 25.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.TopLeft);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxATopRight_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, -10.1f, 42.0f, 25.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.TopRight);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxABottom_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, 3.1f, 42.0f, 38.2f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.Bottom);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxABottomLeft_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, 3.1f, 42.0f, 38.2f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.BottomLeft);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxABottomRight_CorrectResults() {
            BoundingBox expected = new BoundingBox(6.9f, 3.1f, 42.0f, 38.2f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxA, RelativePosition.BottomRight);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxBCenter_CorrectResults() {
            BoundingBox expected = new BoundingBox(-12.3f, -68.4f, 78.9f, 22.8f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxB, RelativePosition.Center);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxBTop_ThrowsException() {
            // The start latitude in this case would be -91.2, which is not allowed.
            Assert.Throws(typeof(Exception), () => BoundingBoxUtils.ExpandToSquare(_boundingBoxB, RelativePosition.Top));
        }

        [Test]
        public void ExpandToSquare_BoundingBoxBBottom_CorrectResults() {
            BoundingBox expected = new BoundingBox(-12.3f, -45.6f, 78.9f, 45.6f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxB, RelativePosition.Bottom);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxC_CorrectResults() {
            // All Positions should return the original bounding box for this test case.
            foreach (RelativePosition position in Enum.GetValues(typeof(RelativePosition))) {
                BoundingBox result = BoundingBoxUtils.ExpandToSquare(_boundingBoxC, position);
                CompareBoundingBoxes(result, _boundingBoxC);
            }
        }

        [Test]
        public void ExpandToSquare_BoundingBoxECenter_CorrectResults() {
            BoundingBox expected = new BoundingBox(-50.0f, -0.0f, 40.0f, 90.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxE, RelativePosition.Center);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxELeft_CorrectResults() {
            BoundingBox expected = new BoundingBox(-20.0f, -0.0f, 70.0f, 90.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxE, RelativePosition.Left);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxERight_CorrectResults() {
            BoundingBox expected = new BoundingBox(-80.0f, -0.0f, 10.0f, 90.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxE, RelativePosition.Right);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void RelativeUV_SameBoundingBox_DefaultResults() {
            UVArea result = BoundingBoxUtils.RelativeUV(_boundingBoxA, _boundingBoxA);
            Assert.AreEqual(UVArea.Default, result);
        }

        [Test]
        public void RelativeUV_BoundingBoxCSameSize_CorrectResults() {
            BoundingBox selectedArea = new BoundingBox(-15.0f, -30.0f, 75.0f, 60.0f);
            UVArea expected = new UVArea(1/3f, 1/3f, 4/3f, 4/3f);
            UVArea actual = BoundingBoxUtils.RelativeUV(_boundingBoxC, selectedArea);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RelativeUV_BoundingBoxCSameAspect_CorrectResults() {
            BoundingBox selectedArea = new BoundingBox(-30.0f, 0.0f, 15.0f, 45.0f);
            UVArea expected = new UVArea(1/6f, 1/2f, 2/3f, 1f);
            UVArea actual = BoundingBoxUtils.RelativeUV(_boundingBoxC, selectedArea);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RelativeUV_BoundingBoxCE_CorrectResults() {
            UVArea expected = new UVArea(5/8f, 0f, 23/24f, 1f);
            UVArea actual = BoundingBoxUtils.RelativeUV(_boundingBoxC, _boundingBoxE);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RelativeUV_BoundingBoxEC_CorrectResults() {
            UVArea expected = new UVArea(-5/6f, 0f, 13/6f, 1f);
            UVArea actual = BoundingBoxUtils.RelativeUV(_boundingBoxE, _boundingBoxC);
            Assert.AreEqual(expected, actual);
        }

        private void CompareBoundingBoxes(BoundingBox a, BoundingBox b) {
            for (int i = 0; i < 4; i++) {
                CompareFloats(a[i], b[i]);
            }
        }

        /// <summary>
        ///     Compares two floating point precision numbers up to the given
        ///     decimal place. Asserts whether they are equal.
        /// </summary>
        /// <param name="a">The first floating point number</param>
        /// <param name="b">The second floating point number</param>
        private void CompareFloats(float a, float b) {
            float multiplier = Mathf.Pow(10, _floatCompareDecimalPrecision);
            a *= multiplier;
            b *= multiplier;
            Assert.AreEqual(Mathf.RoundToInt(a), Mathf.RoundToInt(b));
        }

    }
}

