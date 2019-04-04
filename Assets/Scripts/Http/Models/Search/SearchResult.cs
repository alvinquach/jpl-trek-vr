using System.Collections.Generic;

namespace TrekVRApplication {

    public class SearchResult {

        public int TotalCount { get; }

        public int StartIndex { get; }

        // TODO Make this a IReadOnlyList<>
        public IList<SearchResultItem> Items { get; } = new List<SearchResultItem>();

        public SearchFacetInfo FacetInfo { get; }

        public SearchResult(SearchResponse.Result res) {
            TotalCount = res.response.numFound;
            StartIndex = res.response.start;
            foreach (SearchResponse.Document doc in res.response.docs) {
                Items.Add(new SearchResultItem(doc));
            }
            if (res.facet_counts != null) {
                FacetInfo = new SearchFacetInfo(res.facet_counts.facet_fields);
            }
        }

        public static bool operator true(SearchResult o) {
            return o != null;
        }
        public static bool operator false(SearchResult o) {
            return o == null;
        }
        public static bool operator !(SearchResult o) {
            return o ? false : true;
        }

    }

}
