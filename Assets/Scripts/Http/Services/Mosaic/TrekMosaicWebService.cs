using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    [Obsolete("Digital elevation model and mosaic services have been merged into product service.")]
    public class TrekMosaicWebService : IMosaicWebService {

        private const string BaseUrl = "https://trek.nasa.gov/mars/arcgis/rest/services/Mars_Viking_MDIM21_ClrMosaic_global_232m/ImageServer";

        public static TrekMosaicWebService Instance { get; } = new TrekMosaicWebService();

        private HttpClient _httpClient = HttpClient.Instance;

        public void GetMosaic(BoundingBox bbox, int size, Action<string> callback) {

            TerrainModelProductMetadata metadata = new TerrainModelProductMetadata(GlobalMosaicUUID, bbox, size, ImageFileFormat.Tiff);
            string filename = metadata.EncodeBase64() + ".hi"; // TODO Un-hardcode file extension.

            string directory = Path.Combine(FilePath.PersistentRoot, FilePath.Product);
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
                { "format", "tiff" },
                { "f", "image" }
            };

            string resourceUrl = HttpRequestUtils.AppendParams(BaseUrl + "/exportImage", paramsMap, true);
            Debug.Log(resourceUrl);

            _httpClient.DownloadFile(resourceUrl, filepath, callback);
        }

    }

}