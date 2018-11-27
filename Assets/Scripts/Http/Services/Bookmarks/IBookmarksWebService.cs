using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    public interface IBookmarksWebService {

        void ClearCache();

        void GetBookmarks(Action<IList<Bookmark>> callback);

    }

}
