using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    static class ZFBrowserUtils {

        public static void RegisterFunction(Browser browser, string functionName, Browser.JSCallback function) {
            browser.RegisterFunction($"{UnityGlobalObjectPath}.{functionName}", function);
        }

        public static void NavigateTo(Browser browser, string url) {
            browser.EvalJS($"{AngularGlobalObjectPath}.navigateTo('{url}');");
        }

    }

}
