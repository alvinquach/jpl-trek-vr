using System;

namespace TrekVRApplication {

    public interface IMosaicWebService {

        void GetMosaic(BoundingBox bbox, int size, Action<string> callback);

    }

}