using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public abstract class WebService : MonoBehaviour {

    protected WebServiceManager _webServiceManager { get; private set; }

    #region Callback Function Definitions

    public delegate void ResponseCallback<T>(ResponseContainer<T> res) where T : ResponseObject;

    public delegate void JObjectCallback(JObject res);

    public delegate void TypedObjectCallback<T>(T res);

    public delegate void FileCallback(byte[] res);

    public delegate void ResponseCallback(DownloadHandler res);

    #endregion

    protected virtual void Start() {
        _webServiceManager = WebServiceManager.Instance;
    }

    public abstract void ClearCache();

    // TODO Convert IEnumerator coroutines to async-await functions.

    /// <summary>
    ///     Coroutine for asynchronously making a GET request to a specified API URL.
    ///     The response will be passed back through the callback function.
    /// </summary>
    /// <param name="resourceUrl">
    ///     The URL to the API resource.
    /// </param>
    /// <param name="callback">
    ///     The callback function that is executed when the request is sucessful.
    ///     The response object is passed as a parameter through this function.
    /// </param>
    protected IEnumerator GetCoroutine(string resourceUrl, ResponseCallback callback) {
        UnityWebRequest request = UnityWebRequest.Get(resourceUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else {
            callback(request.downloadHandler);
        }
    }

}
