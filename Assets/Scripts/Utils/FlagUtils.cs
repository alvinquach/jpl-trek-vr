namespace TrekVRApplication {

    public static class FlagUtils {

        public static bool ContainsFlag(int value, int flag) {
            return (value & flag) == flag;
        }

        public static int AddFlag(int value, int flag) {
            return value | flag;
        }

        public static int RemoveFlag(int value, int flag) {
            if (ContainsFlag(value, flag)) {
                value ^= flag;
            }
            return value;
        }

        public static int AddOrRemoveFlag(int value, int flag, bool add) {
            return add ? AddFlag(value, flag) : RemoveFlag(value, flag);
        }

    }

}