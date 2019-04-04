using System.Collections.Generic;

/// <summary>
///     Contains classes that make up the raw bookmark
///     response struture for JSON deserialization.
/// </summary>
namespace TrekVRApplication.BookmarkResponse {

    public class Result {
        public IList<Document> docs;
    }

    public class Document {
        public string bbox;
        public string dem;
        public string description;
        public string item_UUID;
        public string mediaURL;
        public string shape;
        public IList<string> textures;
        public string title;
    }

}
