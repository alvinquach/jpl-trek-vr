using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;

public class BookmarkMenuTest : MonoBehaviour {

    private static readonly string _bookmarksUrl = "https://marstrek.jpl.nasa.gov/TrekServices/ws/index/eq/searchItems?&&&&proj=urn:ogc:def:crs:EPSG::104905&start=0&rows=30&facetKeys=itemType&facetValues=bookmark&&resolutionMin=&resolutionMax=&noSort=false";

    private IList<Bookmark> _bookmarks;

    public GameObject template;

    void OnEnable() {
        if (_bookmarks == null) {
            StartCoroutine(Test());
        }
    }

    IEnumerator Test() {
        UnityWebRequest request = UnityWebRequest.Get(_bookmarksUrl);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError) {
            Debug.Log(request.error);
        }
        else {

            string responseBody = request.downloadHandler.text;

            ResponseContainer<BookmarksResponse> response = JsonConvert.DeserializeObject<ResponseContainer<BookmarksResponse>>(responseBody);

            _bookmarks = response.response.docs;

            if (template != null) {
                for (int i = 0; i < _bookmarks.Count; i++) {
                    Bookmark bookmark = _bookmarks[i];

                    GameObject obj = Instantiate(template, transform);
                    obj.transform.localPosition += 24 * i * Vector3.down;

                    Vector2 centerCoords = CalculateCenterCoordinates(bookmark.bbox);
                    XRMenuElement menuElem = obj.GetComponent<XRMenuElement>();
                    menuElem.latitude = centerCoords.x;
                    menuElem.longitude = centerCoords.y;

                    Text text = obj.GetComponentInChildren<Text>();
                    text.text = bookmark.title;

                    obj.SetActive(true);
                }
            }
        }
    }

    private Vector2 CalculateCenterCoordinates(string coords) {
        // TODO Add sanity checks.
        string[] split = coords.Split(',');
        return new Vector2(
            float.Parse(split[1]) + float.Parse(split[3]),
            float.Parse(split[0]) + float.Parse(split[2])
        ) / 2;
    }

}
