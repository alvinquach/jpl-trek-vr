namespace TrekVRApplication {

    public enum ControllerModalActivity {
        Default,
        BBoxSelection
    }

    public static class ControllerModalActivityEnumExtensions {

        public static string GetModalUrl(this ControllerModalActivity activity) {
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    return "/controller-modal/bbox-selection";
                default:
                    return "/controller-modal";
            }
        }

        public static bool IsPrimaryOnly(this ControllerModalActivity activity) {
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSecondaryOnly(this ControllerModalActivity activity) {
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    return false;
                default:
                    return false;
            }
        }

    }
}