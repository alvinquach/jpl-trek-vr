using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    [Obsolete]
    public class BookmarksResponse : ResponseObject {

        public int numFound;
        public int start;
        public IList<Bookmark> docs;

    }

}
