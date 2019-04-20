using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public enum ControllerModalActivity {
        Default,
        BBoxSelection,
        BookmarkResults,
        NomenclatureResults,
        ProductResults,
        LayerManager,
        ToolsDistance,
        ToolsHeightProfile,
        ToolsSunAngle
    }

    public static class ControllerModalActivityEnumExtensions {

        public static string GetModalUrl(this ControllerModalActivity activity) {
            switch (activity) {
                case ControllerModalActivity.BBoxSelection:
                    return $"{ControllerModalUrl}/bbox-selection";
                case ControllerModalActivity.BookmarkResults:
                    return $"{ControllerModalUrl}/bookmarks";
                case ControllerModalActivity.NomenclatureResults:
                    return $"{ControllerModalUrl}/nomenclatures";
                case ControllerModalActivity.ProductResults:
                    return $"{ControllerModalUrl}/products";
                case ControllerModalActivity.LayerManager:
                    return $"{ControllerModalUrl}/layer-manager";
                case ControllerModalActivity.ToolsDistance:
                    return $"{ControllerModalUrl}/tools/distance";
                case ControllerModalActivity.ToolsHeightProfile:
                    return $"{ControllerModalUrl}/tools/height-profile";
                case ControllerModalActivity.ToolsSunAngle:
                    // TODO Add this
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