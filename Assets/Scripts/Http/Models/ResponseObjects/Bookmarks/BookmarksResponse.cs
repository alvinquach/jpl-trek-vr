using System.Collections.Generic;

namespace TrekVRApplication {

    public class BookmarksResponse : ResponseObject {

        public int numFound;
        public int start;
        public IList<Bookmark> docs;

    }

}
