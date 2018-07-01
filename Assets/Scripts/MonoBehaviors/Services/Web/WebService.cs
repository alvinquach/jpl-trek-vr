using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public abstract class WebService : MonoBehaviour {

    protected WebServiceManager _webServiceManager { get; private set; }

    #region Callback Function Definitions

    // TODO Rename this
    public delegate void VoidCallback();

    public delegate void ResponseCallback<T>(ResponseContainer<T> res) where T : ResponseObject;

    public delegate void JObjectCallback(JObject res);

    public delegate void TypedObjectCallback<T>(T res);

    public delegate void RawBytesCallback(byte[] res);

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
    protected IEnumerator GetBufferCoroutine(string resourceUrl, ResponseCallback callback = null) {
        UnityWebRequest request = UnityWebRequest.Get(resourceUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else if (callback != null) {
            callback(request.downloadHandler);
        }
    }

    /// <summary>
    ///     Coroutine for asynchronously making a GET request to a specified API URL.
    ///     The response will be saved to a file at the specified path.
    /// </summary>
    /// <param name="resourceUrl">
    ///     The URL to the API resource.
    /// </param>
    /// <param name="filePath">
    ///     The path where the response will be saved, relative to the persistent data path.
    /// </param>
    /// <param name="callback">
    ///     The callback function that is executed when the request is sucessful.
    /// </param>
    protected IEnumerator GetFileCoroutine(string resourceUrl, string filePath, VoidCallback callback = null) {
        string path = Path.Combine(Application.persistentDataPath, filePath);
        UnityWebRequest request = new UnityWebRequest(resourceUrl, UnityWebRequest.kHttpVerbGET);
        request.downloadHandler = new DownloadHandlerFile(path);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else if (callback != null) {
            callback();
        }
    }

    /// <summary>
    ///     Coroutine for asynchronously making a GET request to a specified API URL to retreive a texture.
    ///     The response will be converted to a Texture2D and will be passed through the callback function.
    /// </summary>
    /// <param name="resourceUrl">
    ///     The URL to the API resource.
    /// </param>
    /// <param name="callback">
    ///     The callback function that is executed when the request is sucessful.
    ///     The downloaded texture is passed as a parameter through this function.
    /// </param>
    protected IEnumerator GetTextureCoroutine(string resourceUrl, TypedObjectCallback<Texture2D> callback = null) {
        UnityWebRequest request = new UnityWebRequest(resourceUrl, UnityWebRequest.kHttpVerbGET);
        DownloadHandlerTexture downloadHandlerTexture = new DownloadHandlerTexture(true); // TODO Does the texture need to be readable?
        request.downloadHandler = downloadHandlerTexture;
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else if (callback != null) {
            callback(downloadHandlerTexture.texture);
        }
    }



}
