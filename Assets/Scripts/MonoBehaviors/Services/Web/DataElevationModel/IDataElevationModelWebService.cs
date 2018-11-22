using System;

public interface IDataElevationModelWebService {

    void ClearCache();

    void GetDEM(string resourceUrl, string destPath, Action callback);

}
