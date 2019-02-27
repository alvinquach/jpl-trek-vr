using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    public interface ISearchWebService {

        void ClearCache();

        void GetFacetInfo(Action<SearchResult> callback);

        void Search(SearchParameters searchParms, Action<SearchResult> callback);

    }

}
