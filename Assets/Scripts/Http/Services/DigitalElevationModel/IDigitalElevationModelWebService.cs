using System;

namespace TrekVRApplication {

    public interface IDigitalElevationModelWebService {

        void ClearCache();

        void GetDEM(BoundingBox bbox, int size, Action callback);

    }

}
