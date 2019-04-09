namespace TrekVRApplication {

    public static class ServiceManager {

        // Not thread safe on initialization

        public static IBookmarkWebService BookmarkWebService { get; } = MockBookmarkWebService.Instance;

        public static IRasterSubsetWebService RasterSubsetWebService { get; } = TrekRasterSubsetWebService.Instance;

        public static ISearchWebService SearchWebService { get; } = TrekSearchWebService.Instance;

    }

}
