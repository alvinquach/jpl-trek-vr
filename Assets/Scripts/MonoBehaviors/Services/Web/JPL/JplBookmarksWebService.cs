using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class JplBookmarksWebService : BookmarksWebService {

    private IList<Bookmark> _bookmarks;

    public override void ClearCache() {
        _bookmarks = null;
    }

    public override void GetBookmarks(TypedObjectCallback<IList<Bookmark>> callback) {
        if (_bookmarks != null) {
            callback(new List<Bookmark>(_bookmarks));
        }
        else {
            UnityWebRequest request = WebRequestUtils.Get(_webServiceManager.BookmarksUrl);
            BufferRequest(request, (DownloadHandler res) => {
                ResponseContainer<BookmarksResponse> response = JsonConvert.DeserializeObject<ResponseContainer<BookmarksResponse>>(res.text);
                _bookmarks = response.response.docs;
                callback(new List<Bookmark>(_bookmarks));
            });
        }
    }

}
