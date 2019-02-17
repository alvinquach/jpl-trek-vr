using System.Net;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    static class ZFBrowserUtils {

        private static readonly string AngularGlobalObjectPath = $"window['{ZFBrowserConstants.AngularGlobalData}']";
        private static readonly string UnityGlobalObjectPath = $"window['{ZFBrowserConstants.UnityGlobalData}']";

        public static void RegisterStandardFunctions(Browser browser) {

            browser.RegisterFunction($"{UnityGlobalObjectPath}.getRequest", args => {
                string uri = args[0];
                string requestId = args[1];
                Debug.Log(uri);
                Debug.Log(requestId);
                HttpClient.Instance.Get(uri, res => {
                    string responseBody = HttpClient.GetReponseBody((HttpWebResponse)res);
                    Debug.Log(responseBody);
                    SendResponse(browser, requestId, responseBody);
                });

            });

            browser.RegisterFunction($"{UnityGlobalObjectPath}.postRequest", args => {
                string uri = args[0];
                JSONNode body = args[1];
                string requestId = args[2];
                HttpClient.Instance.Post(uri, body, res => {
                    string responseBody = HttpClient.GetReponseBody(res);
                    SendResponse(browser, requestId, responseBody);
                });

            });

            browser.EvalJS($"{UnityGlobalObjectPath}.standardFunctionsReady = true;");
        }

        public static void SendResponse(Browser browser, string requestId, string response) {
            response = response.Replace(@"\", @"\\"); // Need to replace single backslash with double when evaluating JS.
            browser.EvalJS($"{AngularGlobalObjectPath}.fulfillWebRequest(`{requestId}`, `{response}`);");
        }

    }

}
