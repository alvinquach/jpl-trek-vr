using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class UnityWebFunctions : UnityFunctionSet {

        public UnityWebFunctions(Browser browser) : base(browser) {

        }

        [RegisterToBrowser]
        public void GetRequest(string uri, string requestId) {
            HttpClient.Instance.Get(uri, res => {
                string responseBody = HttpClient.GetReponseBody(res);
                SendResponse(_browser, requestId, responseBody);
            });
        }

        public void PostRequest(string uri, JSONNode body, string requestId) {
            HttpClient.Instance.Post(uri, body, res => {
                string responseBody = HttpClient.GetReponseBody(res);
                SendResponse(_browser, requestId, responseBody);
            });
        }

        public override void RegisterFunctions() {
            base.RegisterFunctions();
            _browser.EvalJS($"{UnityGlobalObjectPath}.webFunctionsReady = true;");
        }

        public static void SendResponse(Browser browser, string requestId, string response) {
            response = response.Replace(@"\", @"\\"); // Need to replace single backslash with double when evaluating JS.
            browser.EvalJS($"{AngularGlobalObjectPath}.fulfillWebRequest(`{requestId}`, `{response}`);");
        }

    }

}
