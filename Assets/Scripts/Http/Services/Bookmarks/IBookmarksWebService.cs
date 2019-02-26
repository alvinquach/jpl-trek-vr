using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    [Obsolete("Use the ISearchWebService instead.")]
    public interface IBookmarksWebService {

        void ClearCache();

        void GetBookmarks(Action<IList<Bookmark>> callback);

    }

}
