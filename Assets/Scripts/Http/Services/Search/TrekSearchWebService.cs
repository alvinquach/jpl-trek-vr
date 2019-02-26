using Newtonsoft.Json;
using System;
using TrekVRApplication.SearchResponse;

namespace TrekVRApplication {

    public class TrekSearchWebService : ISearchWebService {

        private const string FacetUrl = "https://trek.nasa.gov/mars/TrekServices/ws/index/eq/searchItems?&&&&proj=urn:ogc:def:crs:EPSG::104905&start=0&rows=0";
        private const string BookmarksUrl = "https://trek.nasa.gov/mars/TrekServices/ws/index/eq/searchItems?&&&&proj=urn:ogc:def:crs:EPSG::104905&start=0&rows=30&facetKeys=itemType&facetValues=bookmark";

        public static TrekSearchWebService Instance { get; } = new TrekSearchWebService();

        #region Cached results

        private Result _facetinfo;
        private Result _bookmarks;

        #endregion

        private TrekSearchWebService() {

        }

        public void ClearCache() {
            _facetinfo = null;
            _bookmarks = null;
        }

        public void GetFacetInfo(Action<Result> callback) {
            if (_facetinfo) {
                callback?.Invoke(_facetinfo);
            }
            HttpClient.Instance.Get(FacetUrl, (res) => {
                string responseBody = HttpClient.GetReponseBody(res);
                _facetinfo = DeserializeReuslts(responseBody);
                callback?.Invoke(_facetinfo);
            });
        }

        public void GetBookmarks(Action<Result> callback) {
            if (_bookmarks) {
                callback?.Invoke(_bookmarks);
            }
            HttpClient.Instance.Get(BookmarksUrl, (res) => {
                string responseBody = HttpClient.GetReponseBody(res);
                _bookmarks = DeserializeReuslts(responseBody);
                callback?.Invoke(_bookmarks);
            });
        }

        public void Search(Action<Result> callback) {
            throw new NotImplementedException();
        }

        public void Search(string search, Action<Result> callback) {
            throw new NotImplementedException();
        }

        private Result DeserializeReuslts(string json) {
            return JsonConvert.DeserializeObject<Result>(json, JsonConfig.SerializerSettings);
        }

    }

}
