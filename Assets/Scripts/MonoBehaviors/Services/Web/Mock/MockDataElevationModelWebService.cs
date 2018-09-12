
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
///     For testing purposes only.
///     Remove this implementation when it is no longer needed.
/// </summary>
public class MockDataElevationModelWebService : DataElevationModelWebService {

    public override void ClearCache() {
        // TODO Implement this
    }

    public override void GetDEM(string resourceUrl, VoidCallback callback) {
        UnityWebRequest request = WebRequestUtils.Post("localhost:8080/rest/files/download", "C:/Users/Alvin/Desktop/temp/asdfasdf.png");
        FileRequest(request, "C:/Users/Alvin/Desktop/temp/dl", callback);
    }

}

