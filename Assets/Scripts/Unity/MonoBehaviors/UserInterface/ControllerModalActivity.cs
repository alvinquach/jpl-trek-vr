namespace TrekVRApplication {

    public enum ControllerModalActivity {
        Default,
        BBoxSelection,
        BookmarkResults,
        ProductResults
    }

    public static class ControllerModalActivityEnumExtensions {

        public static string GetModalUrl(this ControllerModalActivity activity) {
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    return "/controller-modal/bbox-selection";
                case ControllerModalActivity.BookmarkResults:
                    return "/controller-modal/bookmarks";
                case ControllerModalActivity.ProductResults:
                    return "/controller-modal/products";
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