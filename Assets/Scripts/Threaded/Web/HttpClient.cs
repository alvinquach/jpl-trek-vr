using System;
using System.Net;
using System.Threading;

public class HttpClient {

    public void Get(string uri, Action<HttpWebResponse> callback = null) {
        Get(uri, null, callback);
    }

    public void Get(string uri, object options, Action<HttpWebResponse> callback = null) {

        ThreadPool.QueueUserWorkItem((state) => {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                callback?.Invoke(response);
            }

        });

    }

}
