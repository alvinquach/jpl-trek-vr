using Newtonsoft.Json;
using UnityEngine;
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

        public static void SendDataResponse(Browser browser, string requestId, object data) {
            string response = JsonConvert.SerializeObject(data, JsonConfig.SerializerSettings);
            response = response.Replace(@"\", @"\\"); // Need to replace single backslash with double when evaluating JS.
            Debug.Log(response);
            browser.EvalJS($"{AngularInjectableContainerPath}.{DataServiceName}.fulfillRequest(`{requestId}`, `{response}`);");
        }

    }

}
