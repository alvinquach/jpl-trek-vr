using System.Collections.Generic;

/// <summary>
///     Contains classes that make up the raw search
///     response struture for JSON deserialization.
/// </summary>
namespace TrekVRApplication.SearchResponse {

    public class Result {
        public object responseHeader; // TODO Create model for this.
        public Response response;
        public FacetCounts facet_counts;
    }

    public class Response {
        public int numFound;
        public int start;
        public IList<Document> docs;
    }

    public class FacetCounts {
        public object facet_queries; // Empty
        public FacetFields facet_fields;
        public object facet_ranges; // Empty
        public object facet_intervals; // Empty
        public object facet_heatmaps; // Empty
    }

    public class Document {
        public string bbox;
        public string dataProjection;
        public string description;
        public bool hasAttach;
        public string instrument;
        public string item_DBID;
        public string item_UUID;
        public string itemType;
        public string keyword;
        public string productLabel;
        public string productType;
        public string shape;
        public string thumbnailURLDir;
        public string title;
        public long _version_;
    }

    public class FacetFields {
        public List<object> itemType;
        public List<object> productType;
        public List<object> mission;
        public List<object> instrument;
    }

}
