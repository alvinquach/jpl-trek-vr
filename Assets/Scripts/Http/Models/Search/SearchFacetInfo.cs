using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    public class SearchFacetInfo {

        public IDictionary<SearchItemType, int> ItemType { get; } = new Dictionary<SearchItemType, int>();

        public IDictionary<string, int> ProductType { get; } = new Dictionary<string, int>();

        public IDictionary<string, int> Mission { get; } = new Dictionary<string, int>();

        public IDictionary<string, int> Instrument { get; } = new Dictionary<string, int>();

        public SearchFacetInfo(SearchResponse.FacetFields facetFields) {
            for (int i = 0; i < facetFields.itemType.Count; i += 2) {
                SearchItemType itemType = SearchItemTypeEnumExtensions.FromSearchQueryTerm((string)facetFields.itemType[i]);
                long count = (long)facetFields.itemType[i + 1];
                ItemType.Add(itemType, (int)count);
            }
            ConvertListToMap(facetFields.productType, ProductType);
            ConvertListToMap(facetFields.mission, Mission);
            ConvertListToMap(facetFields.instrument, Instrument);
        }

        private void ConvertListToMap(IList<object> src, IDictionary<string, int> dest) {
            // Assume list size is multiple of 2.
            for (int i = 0; i < src.Count; i += 2) {
                string name = (string)src[i];
                long count = (long)src[i + 1];
                dest.Add(name, (int)count);
            }
        }

    }

}
