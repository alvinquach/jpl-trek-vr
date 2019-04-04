using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TrekVRApplication.BookmarkResponse;

namespace TrekVRApplication {

    public class MockBookmarkWebService : IBookmarkWebService {

        // Lazy loaded singleton.
        private static MockBookmarkWebService _instance;
        public static MockBookmarkWebService Instance {
            get {
                if (!_instance) {
                    _instance = new MockBookmarkWebService();
                }
                return _instance;
            }
        }

        private IList<Bookmark> _bookmarks;

        private MockBookmarkWebService() {

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
                MockHttpCall(res => {
                    _bookmarks = DeserializeResults(res);
                    callback(new List<Bookmark>(_bookmarks));
                });
            }
        }

        private IList<Bookmark> DeserializeResults(string json) {
            Result result = JsonConvert.DeserializeObject<Result>(json, JsonConfig.SerializerSettings);
            return result.docs.Select(doc => new Bookmark(doc)).ToList();
        }

        private void MockHttpCall(Action<string> callback) {
            const string json = @"
            {
                ""docs"":[
                    { 
                        ""item_UUID"":""curiosityBookmarkTesting"",
                        ""title"":""Curiosity Bookmark"",
                        ""bbox"":""137.2469,-4.8715,137.5518,-4.5392"",
                        ""shape"":""POLYGON ((137.2469 -4.8715,137.5518 -4.8715,137.5518 -4.5392,137.2469 -4.5392,137.2469 -4.8715))"",
                        ""textures"":[""b40d61ea-a26b-48e1-bdec-5f5ed5cf73d5""],
                        ""dem"":""a0f5221a-0a08-40b9-ae82-75a49aac5afe"",
                        ""description"":""Curiosity landed in Gale Crater on Mars on August 6th, 2012. With a diameter of 154 km and a central peak 5.5 km tall, Gale Crater was chosen as the landing site for the Mars Science Laboratory Curiosity rover. The choice was based on evidence from orbiting spacecraft that indicate that the crater may have once contained large amounts of liquid water. The central peak, Mount Sharp, exhibits layered rock deposits rich in sedimentary minerals including clays, sulfates, and salts that require water to form."",
                        ""mediaURL"":""https://trek.nasa.gov/mars/jpl/assets/features/curiosity/images/curiosity_rover_story.png"",
                    }
                ]
            }";
            callback.Invoke(json);
        }

        public static bool operator true(MockBookmarkWebService o) {
            return o != null;
        }
        public static bool operator false(MockBookmarkWebService o) {
            return o == null;
        }
        public static bool operator !(MockBookmarkWebService o) {
            return o ? false : true;
        }

    }

}
