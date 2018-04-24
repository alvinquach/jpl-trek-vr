using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

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
            StartCoroutine(GetBookmarksCoroutine(callback));
        }
    }

    private IEnumerator GetBookmarksCoroutine(ListResponseCallback<Bookmark> callback) {
        UnityWebRequest request = UnityWebRequest.Get(_webServiceManager.BookmarksUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else {
            string responseBody = request.downloadHandler.text;
            ResponseContainer<BookmarksResponse> response = JsonConvert.DeserializeObject<ResponseContainer<BookmarksResponse>>(responseBody);
            _bookmarks = response.response.docs;
            callback(new List<Bookmark>(_bookmarks));
        }

    }
}
