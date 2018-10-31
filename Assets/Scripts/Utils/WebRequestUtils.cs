using Newtonsoft.Json;
using UnityEngine.Networking;

public sealed class WebRequestUtils {

    private WebRequestUtils() { }

    /// <summary>
    ///     Creates a GET web request. SendWebRequest() can only be
    ///     called once from the returned object. You will need to
    ///     create a new web request to call SendWebRequest() again.
    /// </summary>
    public static UnityWebRequest Get(string resourceUrl) {
        return UnityWebRequest.Get(resourceUrl);
    }

    /// <summary>
    ///     Creates a POST web request. SendWebRequest() can only be
    ///     called once from the returned object. You will need to
    ///     create a new web request to call SendWebRequest() again.
    /// </summary>
    public static UnityWebRequest Post(string resourceUrl, object requestBody) {

        /* 
         * Unity will automaticll URl-encode the request body when creating a POST
         * request directly, which is not what we want here. As a workaround, a PUT
         * request is instead created first, and then the method is changed back to
         * POST after setting the request body and content type.
         */

        UnityWebRequest request = UnityWebRequest.Put(resourceUrl, ToJsonString(requestBody));
        request.SetRequestHeader("Content-Type", GetContentType(requestBody));
        request.method = UnityWebRequest.kHttpVerbPOST;
        return request;
    }


    private static string ToJsonString(object data) {
        if (data is string) {
            return (string)data;
        }
        return JsonConvert.SerializeObject(data);
    }

    private static string GetContentType(object data) {
        if (data is string) {
            return "text/plain";
        }
        return "application/json";
    }

}
 
 
 
 