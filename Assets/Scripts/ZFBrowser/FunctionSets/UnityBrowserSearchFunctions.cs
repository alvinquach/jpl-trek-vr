using Newtonsoft.Json;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class UnityBrowserSearchFunctions : UnityBrowserFunctionSet {

        private ISearchWebService _searchService = TrekSearchWebService.Instance;

        private IRasterSubsetWebService _rasterService = TrekRasterSubsetWebService.Instance;

        private IBookmarkWebService _bookmarkService = MockBookmarkWebService.Instance;

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
            _bookmarkService.GetBookmarks(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetDatasets(string requestId) {
            _searchService.GetDatasets(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetNomenclatures(string requestId) {
            _searchService.GetNomenclatures(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetProducts(string requestId) {
            _searchService.GetProducts(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetRasters(string requestId) {
            _rasterService.GetRasters(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void Search(JSONNode test, string requestId) {
            SearchParameters searchParams = JsonConvert.DeserializeObject<SearchParameters>(test.AsJSON, JsonConfig.SerializerSettings);
            _searchService.Search(searchParams, res => {
                SendResponse(_browser, requestId, res);
            });
        }

        public static void SendResponse(Browser browser, string requestId, object data) {
            string response = JsonConvert.SerializeObject(data, JsonConfig.SerializerSettings);
            response = response.Replace(@"\", @"\\"); // Need to replace single backslash with double when evaluating JS.
            browser.EvalJS($"{AngularInjectableContainerPath}.{SearchServiceName}.fulfillRequest(`{requestId}`, `{response}`);");
        }

    }

}
