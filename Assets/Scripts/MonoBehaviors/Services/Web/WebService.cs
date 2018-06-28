using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public abstract class WebService : MonoBehaviour {

    protected WebServiceManager _webServiceManager { get; private set; }

    #region Callback Function Definitions

    public delegate void ResponseContainerCallback<T>(ResponseContainer<T> res) where T : ResponseObject;

    public delegate void ObjectResponseCallback(JObject res);

    public delegate void TypedObjectResponseCallback<T>(T res);

    public delegate void ListResponseCallback<T>(IList<T> res);

    #endregion

    protected virtual void Start() {
        GameObject obj = GameObject.Find(GameObjectName.Services);
        if (obj != null) {
            _webServiceManager = obj.GetComponentInChildren<WebServiceManager>();
        }
    }

    public abstract void ClearCache();

    protected IEnumerator GetCoroutine<T>(string resourceUrl, ResponseContainerCallback<T> callback) where T : ResponseObject {
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

    protected IEnumerator GetCoroutine(string resourceUrl, ObjectResponseCallback callback) {
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
