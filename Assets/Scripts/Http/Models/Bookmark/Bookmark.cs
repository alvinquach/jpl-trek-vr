using System.Collections.Generic;

namespace TrekVRApplication {

    public class Bookmark {

        public string Name { get; set; }

        public string UUID { get; set; }

        public string ThumbnailUrl { get; set; }

        public string Description { get; set; }

        public string BoundingBox { get; set; }

        public string DemUUID { get; set; }

        public IList<string> TexturesUUID { get; set; }

        // For JSON deserialization
        public Bookmark() {

        }

        public Bookmark(BookmarkResponse.Document doc) {
            Name = doc.title;
            UUID = doc.item_UUID;
            ThumbnailUrl = doc.mediaURL;
            Description = doc.description;
            BoundingBox = doc.bbox;
            DemUUID = doc.dem;
            TexturesUUID = new List<string>(doc.textures);
        }

    }

}
