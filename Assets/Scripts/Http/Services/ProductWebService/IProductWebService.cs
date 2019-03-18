using System;

namespace TrekVRApplication {

    public interface IProductWebService {

        void GetProduct(TerrainModelProductMetadata productInfo, Action<string> callback);

        void GetProduct(TerrainModelProductMetadata productInfo, bool forceRedownload, Action<string> callback);

    }

}
