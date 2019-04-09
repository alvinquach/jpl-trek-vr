using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public static class HttpClient {

        public static string GetReponseBody(HttpWebResponse response) {
            Stream dataStream = response.GetResponseStream();
            using (StreamReader reader = new StreamReader(dataStream)) {
                return reader.ReadToEnd();
            }
        }

        public static void Get(string uri, Action<HttpWebResponse> callback = null, Action<HttpWebResponse> errorCallback = null) {

            ThreadPool.QueueUserWorkItem((state) => {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                SendRequest(request, callback, errorCallback);

            });

        }

        public static void Post(string uri, object body, Action<HttpWebResponse> callback = null, Action<HttpWebResponse> errorCallback = null) {

            ThreadPool.QueueUserWorkItem((state) => {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                SetContent(request, body);

                SendRequest(request, callback, errorCallback);

            });

        }

        public static WebClient DownloadFile(string uri, string filepath, Action<string> callback = null, Action<DownloadProgressChangedEventArgs> progressCallback = null) {

            WebClient client = new WebClient();

            if (callback != null) {
                client.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => callback(filepath));
            }
            if (progressCallback != null) {
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((sender, e) => progressCallback(e));
            }

            client.DownloadFileAsync(new Uri(uri), filepath);
            return client;
        }

        private static void SendRequest(HttpWebRequest request, Action<HttpWebResponse> callback, Action<HttpWebResponse> errorCallback) {

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
        private static void SetContent(HttpWebRequest request, object body) {

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

        private static string ToJsonString(object data) {
            if (data is string) {
                return (string)data;
            }
            else if (data is JSONNode) {
                return ((JSONNode)data).AsJSON;
            }
            return JsonConvert.SerializeObject(data, JsonConfig.SerializerSettings);
        }

        private static string GetContentType(object data) {
            if (data is string) {
                return "text/plain";
            }
            return "application/json";
        }

    }

}
