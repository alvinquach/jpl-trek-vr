using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrekVRApplication {

    public class TrekDigitalElevationModelWebService : IDigitalElevationModelWebService {

        private const string Format = "tiff";

        private const string BaseUrl = "https://trek.nasa.gov/mars/arcgis/rest/services/mola128_mola64_merge_90Nto90S_SimpleC_clon0/ImageServer";

        public static TrekDigitalElevationModelWebService Instance { get; } = new TrekDigitalElevationModelWebService();

        private HttpClient _httpClient = HttpClient.Instance;

        public void GetDEM(BoundingBox bbox, int size, Action<string> callback) {

            TerrainModelFileMetadata metadata = new TerrainModelFileMetadata(0, bbox, size, "tiff");
            string filename = metadata.EncodeBase64() + ".hi"; // TODO Un-hardcode file extension.

            string directory = Path.Combine(FilePath.PersistentRoot, FilePath.DigitalElevationModel);
            IList<string> availableFiles = FileUtils.ListFiles(directory, "*.hi", true); // TODO Un-hardcode file extension.

            string filepath = Path.Combine(directory, filename);
            if (availableFiles.Contains(filename)) {
                callback(filepath);
                return;
            }

            IDictionary<string, string> paramsMap = new Dictionary<string, string>() {
                { "bbox", bbox.ToString(",") },
                { "size", $"{size},{size}" },
                { "noDataInterpretation", "esriNoDataMatchAny" },
                { "interpolation", "RSP_BilinearInterpolation" },
                { "format", Format },
                { "f", "image" }
            };

            string resourceUrl = HttpRequestUtils.AppendParams(BaseUrl + "/exportImage", paramsMap, true);
            Debug.Log(resourceUrl);

            _httpClient.DownloadFile(resourceUrl, filepath, callback);
        }

    }

}
