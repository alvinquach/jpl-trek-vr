using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrekVRApplication {

    [Flags]
    public enum SearchItemType : int {
        Bookmark = 1 << 0,
        Dataset = 1 << 1,
        Nomenclature = 1 << 2,
        Product = 1 << 3
    }

    public static class SearchItemTypeEnumExtensions {

        /// <summary>
        ///     Gets the string representation of the item type that is used
        ///     in the search query. Does not work for compound item types.
        /// </summary>
        public static string GetSearchQueryTerm(this SearchItemType itemType) {
            return StringUtils.FirstCharacterToLower(itemType.ToString());
        }

        public static SearchItemType FromSearchQueryTerm(string queryTerm) {
            if (!Enum.TryParse(StringUtils.FirstCharacterToUpper(queryTerm), out SearchItemType itemType)) {
                Debug.LogError($"{queryTerm} is not a valid item type.");
                return 0;
            }
            return itemType;
        }

    }
}