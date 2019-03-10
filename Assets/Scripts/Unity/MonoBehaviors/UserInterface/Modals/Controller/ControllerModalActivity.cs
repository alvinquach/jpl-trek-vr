using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public enum ControllerModalActivity {
        Default,
        BBoxSelection,
        BookmarkResults,
        ProductResults,
        LayerManager
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
                case ControllerModalActivity.LayerManager:
                    return $"{ControllerModalUrl}/layer-manager";
                default:
                    return $"{ControllerModalUrl}";
            }
        }

        public static bool IsAvailableForPrimary(this ControllerModalActivity activity) {
            switch (activity) {
                case ControllerModalActivity.Default:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAvailableForSecondary(this ControllerModalActivity activity) {
            return true;
        }

    }
}