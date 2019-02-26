namespace TrekVRApplication {

    public class SearchResultItem {

        public string Name { get; }

        public SearchItemType ItemType { get; }

        public SearchResultItem(SearchResponse.Document doc) {
            Name = doc.productLabel;
            ItemType = SearchItemTypeEnumExtensions.FromSearchQueryTerm(doc.itemType);
        }

    }

}
