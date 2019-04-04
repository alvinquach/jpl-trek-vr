using System.Collections.Generic;

namespace TrekVRApplication {

    public class Bookmark {

        public string Name { get; }

        public string UUID { get; }

        public string ThumbnailUrl { get; }

        public string Description { get; }

        public string BoundingBox { get; }

        public string DemUUID { get; }

        public IList<string> TexturesUUID { get; }

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
