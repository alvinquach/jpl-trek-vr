using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class UnityBrowserWebFunctions : UnityBrowserFunctionSet {

        protected override string FunctionsReadyVariable { get; } = "webFunctionsReady";

        public UnityBrowserWebFunctions(Browser browser) : base(browser) {

        }

        [RegisterToBrowser]
        public void GetRequest(string uri, string requestId) {
            HttpClient.Instance.Get(uri, res => {
                string responseBody = HttpClient.GetReponseBody(res);
                SendResponse(_browser, requestId, responseBody);
            });
        }

        [RegisterToBrowser]
        public void PostRequest(string uri, JSONNode body, string requestId) {
            HttpClient.Instance.Post(uri, body, res => {
                string responseBody = HttpClient.GetReponseBody(res);
                SendResponse(_browser, requestId, responseBody);
            });
        }

        public static void SendResponse(Browser browser, string requestId, string response) {
            response = response.Replace(@"\", @"\\"); // Need to replace single backslash with double when evaluating JS.
            browser.EvalJS($"{AngularInjectableContainerPath}.{HttpServiceName}.fulfillRequest(`{requestId}`, `{response}`);");
        }

    }

}
