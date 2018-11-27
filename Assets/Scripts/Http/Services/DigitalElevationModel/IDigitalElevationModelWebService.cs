using App.Geo;
using System;

public interface IDigitalElevationModelWebService {

    void ClearCache();

    void GetDEM(BoundingBox bbox, int size, Action callback);

}
