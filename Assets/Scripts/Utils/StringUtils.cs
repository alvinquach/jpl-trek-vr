namespace TrekVRApplication {

    public static class StringUtils {

        public static string FirstCharacterToLower(string str) {
            if (string.IsNullOrEmpty(str)) {
                return str;
            }
            if (str.Length == 1) {
                return str.ToLower();
            }
            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static string FirstCharacterToUpper(string str) {
            if (string.IsNullOrEmpty(str)) {
                return str;
            }
            if (str.Length == 1) {
                return str.ToUpper();
            }
            return char.ToUpper(str[0]) + str.Substring(1);
        }

    }

}
