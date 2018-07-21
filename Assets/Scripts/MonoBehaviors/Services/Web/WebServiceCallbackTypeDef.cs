using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

// TODO Rename this
public delegate void VoidCallback();

public delegate void ResponseCallback<T>(ResponseContainer<T> res) where T : ResponseObject;

public delegate void JObjectCallback(JObject res);

public delegate void TypedObjectCallback<T>(T res);

public delegate void RawBytesCallback(byte[] res);

public delegate void ResponseCallback(DownloadHandler res);