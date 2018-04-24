using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WebService : MonoBehaviour {

    protected WebServiceManager _webServiceManager { get; private set; }

    #region Callback Function Definitions

    public delegate void ObjectResponseCallback(object res); // TODO Change to JObject?

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
	
}
