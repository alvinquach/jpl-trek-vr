using System;

namespace TrekVRApplication {

    public interface IRasterSubsetWebService {

        void ClearCache();

        void GetRasters(Action<SearchResult> callback, bool forceRefresh = false);

        void GetRaster(string uuid, Action<SearchResultItem> callback, bool forceRefresh = false);

        void SubsetProduct(TerrainProductMetadata productInfo, Action<string> callback);

        void SubsetProduct(TerrainProductMetadata productInfo, bool forceRedownload, Action<string> callback);

    }

}
