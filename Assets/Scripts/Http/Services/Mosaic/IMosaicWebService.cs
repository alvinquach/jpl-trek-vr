using System;

namespace TrekVRApplication {

    [Obsolete("Digital elevation model and mosaic services have been merged into product service.")]
    public interface IMosaicWebService {

        void GetMosaic(BoundingBox bbox, int size, Action<string> callback);

    }

}