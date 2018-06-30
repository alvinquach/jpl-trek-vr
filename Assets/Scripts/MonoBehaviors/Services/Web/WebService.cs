using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public abstract class WebService : MonoBehaviour {

    protected WebServiceManager _webServiceManager { get; private set; }

    #region Callback Function Definitions

    public delegate void ResponseCallback<T>(ResponseContainer<T> res) where T : ResponseObject;

    public delegate void JObjectCallback(JObject res);

    public delegate void TypedObjectCallback<T>(T res);

    public delegate void ListCallback<T>(IList<T> res);

    #endregion

    protected virtual void Start() {
        _webServiceManager = WebServiceManager.Instance;
    }

    public abstract void ClearCache();

    // TODO Convert IEnumerator coroutines to async-await functions.

    /// <summary>
    ///     Coroutine for asynchronously making a GET request to a specified API URL.
    ///     The expected response type is known and is reprsented by a bean class that extends ResponseObject.
    /// </summary>
    /// <typeparam name="T">
    ///     The bean representing the response object. Extends the ResponseObject class.
    /// </typeparam>
    /// <param name="resourceUrl">
    ///     The URL to the API resource.
    /// </param>
    /// <param name="callback">
    ///     The callback function that is executed when the request is sucessful.
    ///     The response object is parsed and passed as a parameter through this function.
    /// </param>
    protected IEnumerator GetCoroutine<T>(string resourceUrl, ResponseCallback<T> callback) where T : ResponseObject {
        UnityWebRequest request = UnityWebRequest.Get(resourceUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else {
            string responseBody = request.downloadHandler.text;
            ResponseContainer<T> response = JsonConvert.DeserializeObject<ResponseContainer<T>>(responseBody);
            callback(response);
        }
    }

    /// <summary>
    ///     Coroutine for asynchronously making a GET request to a specified API URL.
    ///     The expected response type is unknown and will be parsed as a JObject.
    /// </summary>
    /// <param name="resourceUrl">
    ///     The URL to the API resource.
    /// </param>
    /// <param name="callback">
    ///     The callback function that is executed when the request is sucessful.
    ///     The response object is parsed as a JOBject and passed as a parameter through this function.
    /// </param>
    protected IEnumerator GetCoroutine(string resourceUrl, JObjectCallback callback) {
        UnityWebRequest request = UnityWebRequest.Get(resourceUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else {
            string responseBody = request.downloadHandler.text;
            JObject response = (JObject)JsonConvert.DeserializeObject(responseBody);
            callback(response);
        }
    }

}
