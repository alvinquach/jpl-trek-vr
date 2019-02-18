

using NUnit.Framework;
using UnityEngine;

namespace Tests {

    public static class TestUtils {

        /// <summary>
        ///     Compares two floating point precision numbers up to the given
        ///     decimal place. Asserts whether they are equal.
        /// </summary>
        /// <param name="a">The first floating point number</param>
        /// <param name="b">The second floating point number</param>
        public static void CompareFloats(float a, float b, int precision) {
            float multiplier = Mathf.Pow(10, precision);
            a *= multiplier;
            b *= multiplier;
            Assert.AreEqual(Mathf.RoundToInt(a), Mathf.RoundToInt(b));
        }

    }

}
