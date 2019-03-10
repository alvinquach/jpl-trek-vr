namespace TrekVRApplication {

    public class SearchResultItem {

        public string Name { get; }

        public SearchItemType ItemType { get; }

        public string ThumbnailUrl { get; }

        public string ProductLabel { get; }

        public string ProductType { get; }

        public string Instrument { get; }

        public string Description { get; }

        public string BoundingBox { get; }

        public SearchResultItem(SearchResponse.Document doc) {
            Name = doc.title;
            ItemType = SearchItemTypeEnumExtensions.FromSearchQueryTerm(doc.itemType);
            ThumbnailUrl = doc.thumbnailURLDir;
            ProductLabel = doc.productLabel;
            ProductType = doc.productType;
            Instrument = doc.instrument;
            Description = doc.description;
            BoundingBox = doc.bbox;
        }

    }

}
