using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    public class SearchFacetInfo {

        public IList<SearchFacetCount<SearchItemType>> ItemType { get; } = new List<SearchFacetCount<SearchItemType>>();

        public IList<SearchFacetCount<string>> ProductType { get; } = new List<SearchFacetCount<string>>();

        public IList<SearchFacetCount<string>> Mission { get; } = new List<SearchFacetCount<string>>();

        public IList<SearchFacetCount<string>> Instrument { get; } = new List<SearchFacetCount<string>>();

        public SearchFacetInfo(SearchResponse.FacetFields facetFields) {
            for (int i = 0; i < facetFields.itemType.Count; i += 2) {
                SearchItemType itemType = SearchItemTypeEnumExtensions.FromSearchQueryTerm((string)facetFields.itemType[i]);
                SearchFacetCount<SearchItemType> facetCount =
                    new SearchFacetCount<SearchItemType>(itemType, (long)facetFields.itemType[i + 1]);
                ItemType.Add(facetCount);
            }
            ConvertList(facetFields.productType, ProductType);
            ConvertList(facetFields.mission, Mission);
            ConvertList(facetFields.instrument, Instrument);
        }

        private void ConvertList(List<object> src, IList<SearchFacetCount<string>> dest) {
            // Assume list size is multiple of 2.
            for (int i = 0; i < src.Count; i += 2) {
                dest.Add(new SearchFacetCount<string>((string)src[i], (long)src[i + 1]));
            }
        }

    }

}
