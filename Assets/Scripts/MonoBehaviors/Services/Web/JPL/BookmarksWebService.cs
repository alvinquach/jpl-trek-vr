using System.Collections.Generic;

public class BookmarksWebService : WebService {

    private IList<Bookmark> _bookmarks;

    public override void ClearCache() {
        _bookmarks = null;
    }

    public void GetBookmarks(ListResponseCallback<Bookmark> callback) {
        if (_bookmarks != null) {
            callback(new List<Bookmark>(_bookmarks));
        }
        else {
            StartCoroutine(GetCoroutine(_webServiceManager.BookmarksUrl, (ResponseContainer<BookmarksResponse> res) => {
                _bookmarks = res.response.docs;
                callback(new List<Bookmark>(_bookmarks));
            }));
        }
    }

}
