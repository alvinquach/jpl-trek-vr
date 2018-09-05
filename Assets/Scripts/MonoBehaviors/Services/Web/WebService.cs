using System.Collections;
using static System.Exception;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public abstract class WebService {

    protected WebServiceManager _webServiceManager {
        get { return WebServiceManager.Instance; }
    }

    public abstract void ClearCache();

    // TODO Convert IEnumerator coroutines to async-await functions.


    /// <summary>
    ///     Starts the coroutine for asynchronously making an HTTP request.
    ///     The response will be passed back through the callback function.
    /// </summary>
    /// <param name="request">
    ///     The web request object .
    /// </param>
    /// <param name="callback">
    ///     The callback function that is executed when the request is sucessful.
    ///     The response object is passed as a parameter through this function.
    /// </param>
    protected void BufferRequest(UnityWebRequest request, ResponseCallback callback = null) {
        if (RequestAlreadySent(request)) {
            return;
        }
        _webServiceManager.RunCoroutine(BufferRequestCoroutine(request, callback));
    }

    private IEnumerator BufferRequestCoroutine(UnityWebRequest request, ResponseCallback callback) {
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else {
            callback?.Invoke(request.downloadHandler);
        }
    }

    /// <summary>
    ///     Starts the coroutine for asynchronously making a GET request.
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
    protected void FileRequest(UnityWebRequest request, string filePath, VoidCallback callback = null) {
        if (RequestAlreadySent(request)) {
            return;
        }
        _webServiceManager.RunCoroutine(FileRequestCoroutine(request, filePath, callback));
    }

    private IEnumerator FileRequestCoroutine(UnityWebRequest request, string filePath, VoidCallback callback) {
        string path = Path.Combine(Application.persistentDataPath, filePath);
        request.downloadHandler = new DownloadHandlerFile(path);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else {
            callback?.Invoke();
        }
    }

    /// <summary>
    ///     Starts the coroutine for asynchronously making a GET request to retreive a texture.
    ///     The response will be converted to a Texture2D and will be passed through the callback function.
    /// </summary>
    /// <param name="resourceUrl">
    ///     The URL to the API resource.
    /// </param>
    /// <param name="callback">
    ///     The callback function that is executed when the request is sucessful.
    ///     The downloaded texture is passed as a parameter through this function.
    /// </param>
    protected void TextureRequest(UnityWebRequest request, TypedObjectCallback<Texture2D> callback = null) {
        if (RequestAlreadySent(request)) {
            return;
        }
        _webServiceManager.RunCoroutine(TextureRequestCoroutine(request, callback));
    }

    private IEnumerator TextureRequestCoroutine(UnityWebRequest request, TypedObjectCallback<Texture2D> callback) {
        DownloadHandlerTexture downloadHandlerTexture = new DownloadHandlerTexture(true); // TODO Does the texture need to be readable?
        request.downloadHandler = downloadHandlerTexture;
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError) {
            Debug.LogError(request.error);
        }
        else {
            callback?.Invoke(downloadHandlerTexture.texture);
        }
    }

    private bool RequestAlreadySent(UnityWebRequest request) {
        if (!request.isModifiable) {
            Debug.LogWarning("Request was already sent. Please create a new UnityWebRequest to resend the request.");
            return true;
        }
        return false;
    }

}
