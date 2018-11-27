using System;

namespace TrekVRApplication {

    public class TrekDigitalElevationModelWebService : IDigitalElevationModelWebService {

        private const string BaseUrl = "http://ec2-54-177-76-230.us-west-1.compute.amazonaws.com/arcgis/rest/services/mola128_mola64_merge_90Nto90S_SimpleC_clon0/ImageServer";

        public static TrekDigitalElevationModelWebService Instance { get; } = new TrekDigitalElevationModelWebService();

        private HttpClient _httpClient = HttpClient.Instance;

        public void ClearCache() {
            // TODO Implement this
        }

        public void GetDEM(BoundingBox bbox, int size, Action callback) {
            //_httpClient.DownloadFile(resourceUrl, destPath, callback);
        }

    }

}
