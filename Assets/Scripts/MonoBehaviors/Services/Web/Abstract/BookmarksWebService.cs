using System.Collections.Generic;

public abstract class BookmarksWebService : WebService {

    public abstract void GetBookmarks(TypedObjectCallback<IList<Bookmark>> callback);

 }
