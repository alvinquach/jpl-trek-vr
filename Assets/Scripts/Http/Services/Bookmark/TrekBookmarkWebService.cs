using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TrekVRApplication.BookmarkResponse;
using UnityEngine;

namespace TrekVRApplication {

    public class TrekBookmarkWebService : IBookmarkWebService {

        private const string BookmarksUrl = "https://trek.nasa.gov/etc/sampleBookmark.json";

        public static TrekBookmarkWebService Instance { get; } = new TrekBookmarkWebService();

        private IList<Bookmark> _bookmarks;

        private TrekBookmarkWebService() {

        }

        public void ClearCache() {
            _bookmarks = null;
        }

        public void GetBookmarks(Action<IList<Bookmark>> callback, bool forceRefresh = false) {
            if (forceRefresh) {
                _bookmarks = null;
            }
            if (_bookmarks != null) {
                callback(new List<Bookmark>(_bookmarks));
            }
            else {
                HttpClient.Get(BookmarksUrl, res => {
                    string responseBody = HttpClient.GetReponseBody(res);
                    _bookmarks = DeserializeResults(responseBody);
                    callback(new List<Bookmark>(_bookmarks));
                });
            }
        }

        private IList<Bookmark> DeserializeResults(string json) {
            IList<Document> result = JsonConvert.DeserializeObject<IList<Document>>(json, JsonConfig.SerializerSettings);
            return result.Select(doc => new Bookmark(doc)).ToList();
        }

    }

}
