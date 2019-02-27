using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TrekVRApplication.SearchResponse;

namespace TrekVRApplication {

    public class TrekSearchWebService : ISearchWebService {

        private const string BaseUrl = "https://trek.nasa.gov/mars/TrekServices/ws/index/eq/searchItems?&&&&proj=urn:ogc:def:crs:EPSG::104905";

        public static TrekSearchWebService Instance { get; } = new TrekSearchWebService();

        #region Cached results

        private SearchResult _facetinfo;
        private SearchResult _bookmarks;

        #endregion

        private TrekSearchWebService() {

        }

        public void ClearCache() {
            _facetinfo = null;
            _bookmarks = null;
        }

        public void GetFacetInfo(Action<SearchResult> callback) {
            if (_facetinfo) {
                callback?.Invoke(_facetinfo);
                return;
            }

            IDictionary<string, string> paramsMap = new Dictionary<string, string>() {
                { "start", "0" },
                { "rows", "0" } // Only get the facet data.
            };

            string searchUrl = HttpRequestUtils.AppendParams(BaseUrl, paramsMap);

            HttpClient.Instance.Get(searchUrl, (res) => {
                string responseBody = HttpClient.GetReponseBody(res);
                _facetinfo = DeserializeReuslts(responseBody);
                callback?.Invoke(_facetinfo);
            });
        }

        public void Search(SearchParameters searchParams, Action<SearchResult> callback) {

            GetFacetInfo(data => {

                IDictionary<string, string> paramsMap = new Dictionary<string, string>() {
                    { "start", "0" } // Always start at index 0.
                };

                // Get the total count of the item type from the facet info.
                // If a count doesn't exist for the item type, then the count
                // is the total number of all items.
                if (!data.FacetInfo.ItemType.TryGetValue(searchParams.ItemType, out int itemCount)) {
                    itemCount = data.FacetInfo.ItemType.Values.Aggregate(0, (sum, val) => sum + val);
                }
                paramsMap.Add("rows", itemCount.ToString());

                string facetKeys = "", facetValues = "";
                if (searchParams.ItemType > 0) {
                    AppendFacetQuery(ref facetKeys, "itemType");
                    AppendFacetQuery(ref facetValues, searchParams.ItemType.GetSearchQueryTerm());
                }
                if (!string.IsNullOrEmpty(searchParams.ProductType)) {
                    AppendFacetQuery(ref facetKeys, "productType");
                    AppendFacetQuery(ref facetValues, searchParams.ProductType);
                }
                if (!string.IsNullOrEmpty(searchParams.Mission)) {
                    AppendFacetQuery(ref facetKeys, "mission");
                    AppendFacetQuery(ref facetValues, searchParams.Mission);
                }
                if (!string.IsNullOrEmpty(searchParams.Instrument)) {
                    AppendFacetQuery(ref facetKeys, "instrument");
                    AppendFacetQuery(ref facetValues, searchParams.Instrument);
                }
                paramsMap.Add("facetKeys", facetKeys);
                paramsMap.Add("facetValues", facetValues);

                if (!string.IsNullOrEmpty(searchParams.Search)) {
                    paramsMap.Add("key", searchParams.Search);
                }

                string searchUrl = HttpRequestUtils.AppendParams(BaseUrl, paramsMap);

                HttpClient.Instance.Get(searchUrl, (res) => {
                    string responseBody = HttpClient.GetReponseBody(res);
                    SearchResult searchResult = DeserializeReuslts(responseBody);
                    callback?.Invoke(searchResult);
                });

            });

        }

        private SearchResult DeserializeReuslts(string json) {
            Result result = JsonConvert.DeserializeObject<Result>(json, JsonConfig.SerializerSettings);
            return new SearchResult(result);
        }

        private void AppendFacetQuery(ref string dest, string add) {
            dest = dest + (dest.Length > 0 ? "|" : "") + add;
        }

    }

}
