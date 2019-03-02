using static TrekVRApplication.ZFBrowserConstants;

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
                    return $"{ControllerModalUrl}/bbox-selection";
                case ControllerModalActivity.BookmarkResults:
                    return $"{ControllerModalUrl}/bookmarks";
                case ControllerModalActivity.ProductResults:
                    return $"{ControllerModalUrl}/products";
                default:
                    return $"{ControllerModalUrl}";
            }
        }

        public static bool IsAvailableForPrimary(this ControllerModalActivity activity) {
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    return false;
                default:
                    return true;
            }
        }

        public static bool IsAvailableForSecondary(this ControllerModalActivity activity) {
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    return true;
                default:
                    return true;
            }
        }

    }
}