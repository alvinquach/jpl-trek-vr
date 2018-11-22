using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class HttpClient {

    public static HttpClient Instance { get; } = new HttpClient();

    private HttpClient() { }

    public static string GetReponseBody(HttpWebResponse response) {
        Stream dataStream = response.GetResponseStream();
        using (StreamReader reader = new StreamReader(dataStream)) {
            return reader.ReadToEnd();
        }
    }

    public void Get(string uri, Action<HttpWebResponse> callback = null, Action<HttpWebResponse> errorCallback = null) {

        ThreadPool.QueueUserWorkItem((state) => {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            SendRequest(request, callback, errorCallback);

        });

    }

    public void Post(string uri, object body, Action<HttpWebResponse> callback = null, Action<HttpWebResponse> errorCallback = null) {

        ThreadPool.QueueUserWorkItem((state) => {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            SetContent(request, body);

            SendRequest(request, callback, errorCallback);

        });

    }

    private void SendRequest(HttpWebRequest request, Action<HttpWebResponse> callback, Action<HttpWebResponse> errorCallback) {

        try {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                callback?.Invoke(response);
            }
        }

        catch (WebException e) {
            using (HttpWebResponse response = (HttpWebResponse)e.Response) {
                if (errorCallback != null) {
                    errorCallback(response);
                }
                else {
                    Debug.LogError(e.Message);
                }
            }
        }

    }

    /// <summary>
    ///     Helper method to set the request body for POST, PUT, and PATCH requests.
    /// </summary>
    /// <param name="request">The HttpWebRequest object.</param>
    /// <param name="body">The request body data.</param>
    private void SetContent(HttpWebRequest request, object body) {

        // Convert body object into bytes
        string contentString = ToJsonString(body);
        byte[] contentBytes = Encoding.UTF8.GetBytes(contentString);

        // Set content type and content length
        request.ContentType = GetContentType(body);
        request.ContentLength = contentBytes.Length;

        // Wirte content to request stream
        using (Stream dataStream = request.GetRequestStream()) {
            dataStream.Write(contentBytes, 0, contentBytes.Length);
        }

    }

    private string ToJsonString(object data) {
        if (data is string) {
            return (string)data;
        }
        return JsonConvert.SerializeObject(data);
    }

    private string GetContentType(object data) {
        if (data is string) {
            return "text/plain";
        }
        return "application/json";
    }

}
