using System.Collections.Generic;
using UnityEngine;

public class TempKeyboardInputController : MonoBehaviour {

    private int count = 0;

    void Update() {

        if (Input.GetKeyUp(KeyCode.A)) {
            WebServiceManager.Instance?.BookmarksWebService.GetBookmarks(OnGetBookmarks);
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            WebServiceManager.Instance?.DataElevationModelWebService.GetDEM(null, null);
        }

    }

    private void OnGetBookmarks(IList<Bookmark> bookmarks) {
        Debug.Log("Hello " + ++count + "; count=" + bookmarks.Count);
    }

}