using System;

public class JplDigitalElevationModelWebService : IDigitalElevationModelWebService {

    public static JplDigitalElevationModelWebService Instance { get; } = new JplDigitalElevationModelWebService();

    public void ClearCache() {
        // TODO Implement this
    }

    public void GetDEM(string resourceUrl, string destPath, Action callback) {
        // TODO Implement this
    }

}
