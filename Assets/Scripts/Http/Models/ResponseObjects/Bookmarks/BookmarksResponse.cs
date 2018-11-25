using System.Collections.Generic;

public class BookmarksResponse : ResponseObject {

    public int numFound;
    public int start;
    public IList<Bookmark> docs;

}
