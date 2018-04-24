using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebServiceManager : MonoBehaviour {

    // TODO Move this to constants file.
    public readonly string BookmarksUrl = "https://marstrek.jpl.nasa.gov/TrekServices/ws/index/eq/searchItems?&&&&proj=urn:ogc:def:crs:EPSG::104905&start=0&rows=30&facetKeys=itemType&facetValues=bookmark";

    #region Web Services

    private BookmarksWebService _bookmarksWebService;
    public BookmarksWebService BookmarksWebService {
        get { return _bookmarksWebService; }
    }

    #endregion

    void Start () {
        _bookmarksWebService = gameObject.AddComponent<BookmarksWebService>();
	}
	
}
