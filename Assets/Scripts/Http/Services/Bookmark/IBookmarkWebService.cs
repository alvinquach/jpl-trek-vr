using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    public interface IBookmarkWebService {

        void ClearCache();

        void GetBookmarks(Action<IList<Bookmark>> callback, bool forceRefresh = false);

    }

}
