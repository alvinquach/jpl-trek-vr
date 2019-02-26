using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    [Obsolete("Use the TrekSearchWebService instead.")]
    public class TrekBookmarksWebService : IBookmarksWebService {

        public static TrekBookmarksWebService Instance { get; } = new TrekBookmarksWebService();

        // TODO Move this to constants file.
        private const string BookmarksUrl = "https://marstrek.jpl.nasa.gov/TrekServices/ws/index/eq/searchItems?&&&&proj=urn:ogc:def:crs:EPSG::104905&start=0&rows=30&facetKeys=itemType&facetValues=bookmark";

        private HttpClient _httpClient = HttpClient.Instance;

        private IList<Bookmark> _bookmarks;

        public void ClearCache() {
            _bookmarks = null;
        }

        public void GetBookmarks(Action<IList<Bookmark>> callback) {
            if (_bookmarks != null) {
                callback(new List<Bookmark>(_bookmarks));
            }
            else {
                _httpClient.Get(BookmarksUrl, (res) => {
                    string responseBody = HttpClient.GetReponseBody(res);

                    ResponseContainer<BookmarksResponse> response = JsonConvert.DeserializeObject<ResponseContainer<BookmarksResponse>>(responseBody);
                    _bookmarks = response.response.docs;
                    callback(new List<Bookmark>(_bookmarks));
                });

            }
        }

    }

}
