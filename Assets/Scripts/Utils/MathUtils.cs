using UnityEngine;

namespace TrekVRApplication {

    public static class MathUtils {

        public static int Clamp(int value, int min, int max) {
            return value < min ? min : value > max ? max : value;
        }

        // Source: https://stackoverflow.com/questions/600293/how-to-check-if-a-number-is-a-power-of-2
        public static bool IsPowerOfTwo(int x) {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        /// <summary>
        ///     Wraps an angle to a value between 0 and 360 degrees.
        /// </summary>
        /// <param name="angle">Angle in degress.</param>
        /// <returns>The equivalent angle between 0 and 360 degrees.</returns>
        public static float WrapAngle0To360(float angle) {
            return angle % 360f;
        }

        /// <summary>
        ///     Wraps an angle to a value between -180 and 180 degrees.
        /// </summary>
        /// <param name="angle">Angle in degress.</param>
        /// <returns>The equivalent angle between -180 and 180 degrees.</returns>
        public static float WrapAngle180(float angle) {
            return (angle + 180) % 360f - 180f;
        }

        /// <summary>
        ///     Wraps an angle to a value between 0 and 2PI radians.
        /// </summary>
        /// <param name="angle">Angle in radians.</param>
        /// <returns>The equivalent angle between 0 and 2PI radians.</returns>
        public static float WrapAngle0To2Pi(float angle) {
            return angle % (2 * Mathf.PI);
        }

        /// <summary>
        ///     Wraps an angle to a value between -PI and PI radians.
        /// </summary>
        /// <param name="angle">Angle in radians.</param>
        /// <returns>The equivalent angle between -PI and PI radians.</returns>
        public static float WrapAnglePi(float angle) {
            return (angle + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
        }

    }

}