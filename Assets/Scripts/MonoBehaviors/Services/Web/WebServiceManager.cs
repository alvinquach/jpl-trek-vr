using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class WebServiceManager : MonoBehaviour {

    // TODO Move this to constants file.
    public readonly string BookmarksUrl = "https://marstrek.jpl.nasa.gov/TrekServices/ws/index/eq/searchItems?&&&&proj=urn:ogc:def:crs:EPSG::104905&start=0&rows=30&facetKeys=itemType&facetValues=bookmark";

    /// <summary>
    ///     The instance of the WebServiceManager that is present in the scene.
    ///     There should only be one WebServiceManager in the entire scene.
    /// </summary>
    public static WebServiceManager Instance { get; private set; }

    #region Web Services

    public BookmarksWebService BookmarksWebService { get; private set; }

    public DataElevationModelWebService DataElevationModelWebService { get; private set; }

    #endregion

    void Awake () {

        if (Instance == null) {
            Instance = this;
        }

        BookmarksWebService = new JplBookmarksWebService();
        DataElevationModelWebService = new MockDataElevationModelWebService();
    }

    /// <summary>
    ///     Helper method for WebService classes to run their request coroutines;
    ///     WebService does not extend MonoBehaviour and thus cannot run its own coroutines.
    /// </summary>
    /// <param name="routine">The routine to run.</param>
    /// <returns>The coroutine that was run.</returns>
    public Coroutine RunCoroutine(IEnumerator routine) {
        return StartCoroutine(routine);
    }
	
}
