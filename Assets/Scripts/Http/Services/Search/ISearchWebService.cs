using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    public interface ISearchWebService {

        void ClearCache();

        void GetFacetInfo(Action<SearchResponse.Result> callback);

        void GetBookmarks(Action<SearchResponse.Result> callback);

        void Search(Action<SearchResponse.Result> callback);

        void Search(string search, Action<SearchResponse.Result> callback);

    }

}
