public static class MathUtils {

    public static int Clamp(int value, int min, int max) {
        return value < min ? min : value > max ? max : value;
    }

    // Source: https://stackoverflow.com/questions/600293/how-to-check-if-a-number-is-a-power-of-2
    public static bool IsPowerOfTwo(int x) {
        return (x != 0) && ((x & (x - 1)) == 0);
    }

}