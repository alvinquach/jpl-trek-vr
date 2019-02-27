using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    public interface ISearchWebService {

        void ClearCache();

        void GetFacetInfo(Action<SearchResult> callback, bool forceRefresh = false);

        void GetBookmarks(Action<SearchResult> callback, bool forceRefresh = false);

        void GetDatasets(Action<SearchResult> callback, bool forceRefresh = false);

        void GetNomenclature(Action<SearchResult> callback, bool forceRefresh = false);

        void GetProducts(Action<SearchResult> callback, bool forceRefresh = false);

        void Search(SearchParameters searchParms, Action<SearchResult> callback);

    }

}
