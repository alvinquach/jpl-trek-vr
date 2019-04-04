using System;

namespace TrekVRApplication {

    public interface IRasterSubsetWebService {

        void GetRasters(Action<SearchResult> callback, bool forceRefresh = false);

        void SubsetProduct(TerrainModelProductMetadata productInfo, Action<string> callback);

        void SubsetProduct(TerrainModelProductMetadata productInfo, bool forceRedownload, Action<string> callback);

    }

}
