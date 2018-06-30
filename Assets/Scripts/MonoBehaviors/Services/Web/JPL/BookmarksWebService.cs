using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class BookmarksWebService : WebService {

    private IList<Bookmark> _bookmarks;

    public override void ClearCache() {
        _bookmarks = null;
    }

    public void GetBookmarks(TypedObjectCallback<IList<Bookmark>> callback) {
        if (_bookmarks != null) {
            callback(new List<Bookmark>(_bookmarks));
        }
        else {
            StartCoroutine(GetCoroutine(_webServiceManager.BookmarksUrl, (DownloadHandler res) => {
                ResponseContainer<BookmarksResponse> response = JsonConvert.DeserializeObject<ResponseContainer<BookmarksResponse>>(res.text);
                _bookmarks = response.response.docs;
                callback(new List<Bookmark>(_bookmarks));
            }));
        }
    }

}
