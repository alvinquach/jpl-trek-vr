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
        private readonly BoundingBox _boundingBoxF = new BoundingBox(-135.0f, -30.0f, 135.0f, 30.0f);

        [Test]
        public void ParseBoundingBox_BoundingBoxA_ParsesCorrectly() {
            string boundingBoxString = "6.90,3.10,42.00,25.00";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(_boundingBoxA, boundingBox);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxAReversed_ParsesCorrectly() {
            string boundingBoxString = "42.00,25.00,6.90,3.10";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(_boundingBoxA, boundingBox);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxB_ParsesCorrectly() {
            string boundingBoxString = "-12.3,-45.6,78.9,0";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(_boundingBoxB, boundingBox);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxBReversed_ParsesCorrectly() {
            string boundingBoxString = "78.9,0,-12.3,-45.6";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(_boundingBoxB, boundingBox);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxCLatWrapped_ParsesCorrectly() {
            string boundingBoxString = "-45,0,45,99";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(_boundingBoxC, boundingBox);
        }

        [Test]
        public void ParseBoundingBox_BoundingBoxCLongWrapped_ParsesCorrectly() {
            string boundingBoxString = "315,0,45,90";
            BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(boundingBoxString);
            Assert.AreEqual(_boundingBoxC, boundingBox);
        }

        [Test]
        public void MedianLatLon_BoundingBoxA_CorrectResult() {
            Vector2 expected = new Vector2(14.05f, 24.45f);
            Vector2 actual = BoundingBoxUtils.MedianLatLon(_boundingBoxA);
            for (int i = 0; i < 2; i++) {
                TestUtils.CompareFloats(expected[i], actual[i], _floatCompareDecimalPrecision);
            }
        }

        [Test]
        public void MedianLatLon_BoundingBoxB_CorrectResult() {
            Vector2 expected = new Vector2(-22.8f, 33.3f);
            Vector2 actual = BoundingBoxUtils.MedianLatLon(_boundingBoxB);
            for (int i = 0; i < 2; i++) {
                TestUtils.CompareFloats(expected[i], actual[i], _floatCompareDecimalPrecision);
            }
        }

        [Test]
        public void MedianLatLon_BoundingBoxD_CorrectResult() {
            Vector2 expected = new Vector2(-45.0f, -45.0f);
            Vector2 actual = BoundingBoxUtils.MedianLatLon(_boundingBoxD);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MedianLatLon_BoundingBoxF_CorrectResult() {
            Vector2 expected = new Vector2(0.0f, 180.0f);
            Vector2 actual = BoundingBoxUtils.MedianLatLon(_boundingBoxF);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MedianDirection_BoundingBoxD_CorrectResult() {
            Vector3 expected = new Vector3(0.5f, -Mathf.Sqrt(0.5f), 0.5f);
            Vector3 actual = BoundingBoxUtils.MedianDirection(_boundingBoxD);
            for (int i = 0; i < 3; i++) {
                TestUtils.CompareFloats(expected[i], actual[i], _floatCompareDecimalPrecision);
            }
        }

        [Test]
        public void ExpandToSquare_BoundingBoxB_CorrectResults() {
            BoundingBox expected = new BoundingBox(-12.3f, -90.0f, 78.9f, 1.2f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxB);
            CompareBoundingBoxes(expected, actual);
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
        public void ExpandToSquare_BoundingBoxFCenter_CorrectResults() {
            BoundingBox expected = new BoundingBox(-135.0f, -45.0f, 135.0f, 45.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxF, RelativePosition.Center);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxFTop_CorrectResults() {
            BoundingBox expected = new BoundingBox(-135.0f, -60.0f, 135.0f, 30.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxF, RelativePosition.Top);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void ExpandToSquare_BoundingBoxFBottom_CorrectResults() {
            BoundingBox expected = new BoundingBox(-135.0f, -30.0f, 135.0f, 60.0f);
            BoundingBox actual = BoundingBoxUtils.ExpandToSquare(_boundingBoxF, RelativePosition.Bottom);
            CompareBoundingBoxes(expected, actual);
        }

        [Test]
        public void CalculateUVBounds_SameBoundingBox_DefaultResults() {
            UVBounds result = BoundingBoxUtils.CalculateUVBounds(_boundingBoxA, _boundingBoxA);
            Assert.AreEqual(UVBounds.Default, result);
        }

        [Test]
        public void CalculateUVBounds_BoundingBoxCSameSize_CorrectResults() {
            BoundingBox selectedArea = new BoundingBox(-15.0f, -30.0f, 75.0f, 60.0f);
            UVBounds expected = new UVBounds(1/3f, 1/3f, 4/3f, 4/3f);
            UVBounds actual = BoundingBoxUtils.CalculateUVBounds(_boundingBoxC, selectedArea);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateUVBounds_BoundingBoxCSameAspect_CorrectResults() {
            BoundingBox selectedArea = new BoundingBox(-30.0f, 0.0f, 15.0f, 45.0f);
            UVBounds expected = new UVBounds(1/6f, 1/2f, 2/3f, 1f);
            UVBounds actual = BoundingBoxUtils.CalculateUVBounds(_boundingBoxC, selectedArea);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateUVBounds_BoundingBoxCE_CorrectResults() {
            UVBounds expected = new UVBounds(5/18f, 0f, 11/18f, 1f);
            UVBounds actual = BoundingBoxUtils.CalculateUVBounds(_boundingBoxC, _boundingBoxE);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateUVBounds_BoundingBoxEC_CorrectResults() {
            UVBounds expected = new UVBounds(-5/6f, 0f, 13/6f, 1f);
            UVBounds actual = BoundingBoxUtils.CalculateUVBounds(_boundingBoxE, _boundingBoxC);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateUVBounds_BoundingBoxFSameSize1_CorrectResults() {
            new BoundingBox(-135.0f, -30.0f, 135.0f, 30.0f);
            BoundingBox selectedArea = new BoundingBox(90.0f, -30.0f, 180.0f, 30.0f);
            UVBounds expected = new UVBounds(-1/2f, 0f, 1/2f, 1f);
            UVBounds actual = BoundingBoxUtils.CalculateUVBounds(_boundingBoxF, selectedArea);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateUVBounds_BoundingBoxFSameSize2_CorrectResults() {
            BoundingBox selectedArea = new BoundingBox(120.0f, 0.0f, -150.0f, 60.0f);
            UVBounds expected = new UVBounds(-1/6f, -1/2f, 5/6f, 1/2f);
            UVBounds actual = BoundingBoxUtils.CalculateUVBounds(_boundingBoxF, selectedArea);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void IsLongitudeWrapped_BoundingBoxA_False() {
            Assert.IsFalse(BoundingBoxUtils.IsLongitudeWrapped(_boundingBoxA));
        }

        [Test]
        public void IsLongitudeWrapped_BoundingBoxF_True() {
            Assert.IsTrue(BoundingBoxUtils.IsLongitudeWrapped(_boundingBoxF));
        }

        [Test]
        public void IsLongitudeWrapped_Global_False() {
            Assert.IsFalse(BoundingBoxUtils.IsLongitudeWrapped(UnrestrictedBoundingBox.Global));
        }

        private void CompareBoundingBoxes(BoundingBox a, BoundingBox b) {
            for (int i = 0; i < 4; i++) {
                TestUtils.CompareFloats(a[i], b[i], _floatCompareDecimalPrecision);
            }
        }

    }
}

