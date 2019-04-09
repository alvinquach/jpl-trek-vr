using Newtonsoft.Json;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ServiceManager;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class UnityBrowserSearchFunctions : UnityBrowserFunctionSet {

        protected override string FunctionsReadyVariable { get; } = "searchFunctionsReady";

        public UnityBrowserSearchFunctions(Browser browser) : base(browser) {
            // TODO Unsubscribe on destroy
            SearchWebService.OnSearchListActiveIndexChange += SendSearchListActiveIndex;
        }

        public override void RegisterFunctions() {
            SendSearchListActiveIndex(SearchWebService.SearchListActiveIndex);
            base.RegisterFunctions();
        }

        [RegisterToBrowser]
        public void UpdateSearchListActiveIndex(double? index) {
            if (index == null) {
                SearchWebService.SearchListActiveIndex = null;
            } else {
                SearchWebService.SearchListActiveIndex = (int)index;
            }
        }

        [RegisterToBrowser]
        public void GetFacetInfo(string requestId) {
            SearchWebService.GetFacetInfo(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetBookmarks(string requestId) {
            BookmarkWebService.GetBookmarks(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetDatasets(string requestId) {
            SearchWebService.GetDatasets(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetNomenclatures(string requestId) {
            SearchWebService.GetNomenclatures(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetProducts(string requestId) {
            SearchWebService.GetProducts(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void GetRasters(string requestId) {
            RasterSubsetWebService.GetRasters(res => {
                SendResponse(_browser, requestId, res);
            });
        }

        [RegisterToBrowser]
        public void Search(JSONNode test, string requestId) {
            SearchParameters searchParams = JsonConvert.DeserializeObject<SearchParameters>(test.AsJSON, JsonConfig.SerializerSettings);
            SearchWebService.Search(searchParams, res => {
                SendResponse(_browser, requestId, res);
            });
        }

        public void SendSearchListActiveIndex(int? index) {
            _browser.EvalJS($"{UnityGlobalObjectPath}.onSearchListActiveIndexChange.next({index});");
        }

        public static void SendResponse(Browser browser, string requestId, object data) {
            string response = JsonConvert.SerializeObject(data, JsonConfig.SerializerSettings);
            response = response.Replace(@"\", @"\\"); // Need to replace single backslash with double when evaluating JS.
            browser.EvalJS($"{AngularInjectableContainerPath}.{SearchServiceName}.fulfillRequest(`{requestId}`, `{response}`);");
        }

    }

}
