using System;

public interface IDigitalElevationModelWebService {

    void ClearCache();

    void GetDEM(string resourceUrl, string destPath, Action callback);

}
