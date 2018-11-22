using System;

public class JplDataElevationModelWebService : IDataElevationModelWebService {

    public static JplDataElevationModelWebService Instance { get; } = new JplDataElevationModelWebService();

    public void ClearCache() {
        // TODO Implement this
    }

    public void GetDEM(string resourceUrl, string destPath, Action callback) {
        // TODO Implement this
    }

}
