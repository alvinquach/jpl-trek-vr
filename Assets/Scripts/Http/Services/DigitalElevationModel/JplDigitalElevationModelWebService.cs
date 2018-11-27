using App.Http.Utils;
using System;

public class JplDigitalElevationModelWebService : IDigitalElevationModelWebService {

    public static JplDigitalElevationModelWebService Instance { get; } = new JplDigitalElevationModelWebService();

    private HttpClient _httpClient = HttpClient.Instance;

    public void ClearCache() {
        // TODO Implement this
    }

    public void GetDEM(string resourceUrl, string destPath, Action callback) {
        _httpClient.DownloadFile(resourceUrl, destPath, callback);
    }

}
