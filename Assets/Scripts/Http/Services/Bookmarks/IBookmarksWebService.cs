using System;
using System.Collections.Generic;

public interface IBookmarksWebService {

    void ClearCache();

    void GetBookmarks(Action<IList<Bookmark>> callback);

}
