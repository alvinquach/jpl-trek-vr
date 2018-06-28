using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WebServiceManager : MonoBehaviour {

    // TODO Move this to constants file.
    public readonly string BookmarksUrl = "https://marstrek.jpl.nasa.gov/TrekServices/ws/index/eq/searchItems?&&&&proj=urn:ogc:def:crs:EPSG::104905&start=0&rows=30&facetKeys=itemType&facetValues=bookmark";

    private static WebServiceManager _instance;

    /// <summary>
    ///     The instance of the WebServiceManager that is present in the scene.
    ///     There should only be one WebServiceManager in the entire scene.
    /// </summary>
    public static WebServiceManager Instance {
        get { return _instance; }
    }

    #region Web Services

    private BookmarksWebService _bookmarksWebService;
    public BookmarksWebService BookmarksWebService {
        get { return _bookmarksWebService; }
    }

    #endregion

    void Awake () {
        _bookmarksWebService = gameObject.AddComponent<BookmarksWebService>();

        if (WebServiceManager._instance == null) {
            WebServiceManager._instance = this;
        }
	}
	
}
