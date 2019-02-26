using Newtonsoft.Json;
using TrekVRApplication.SearchResponse;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class UnityBrowserSearchFunctions : UnityBrowserFunctionSet {

        private ISearchWebService _searchService = TrekSearchWebService.Instance;

        protected override string FunctionsReadyVariable { get; } = "searchFunctionsReady";

        public UnityBrowserSearchFunctions(Browser browser) : base(browser) {

        }

        [RegisterToBrowser]
        public void GetFacetInfo(string requestId) {
            _searchService.GetFacetInfo(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetBookmarks(string requestId) {
            _searchService.GetBookmarks(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        public static void SendResponse(Browser browser, string requestId, Result data) {
            string response = JsonConvert.SerializeObject(new SearchResult(data), JsonConfig.SerializerSettings);
            response = response.Replace(@"\", @"\\"); // Need to replace single backslash with double when evaluating JS.
            browser.EvalJS($"{AngularInjectableContainerPath}.{SearchServiceName}.fulfillSearchRequest(`{requestId}`, `{response}`);");
        }

    }

}
