using System;

namespace TrekVRApplication {

    public interface IDigitalElevationModelWebService {

        void GetDEM(BoundingBox bbox, int size, Action<string> callback);

    }

}
