using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Implementation of IProductWebService that retrieves products from the
    ///     Trek web services.
    /// </summary>
    public class TrekProductWebService : IProductWebService {

        public static TrekProductWebService Instance { get; } = new TrekProductWebService();

        private const string BaseUrl = "https://trek.nasa.gov/marsbeta/TrekServices/ws/raster";

        private HttpClient _httpClient = HttpClient.Instance;

        /// <summary>
        ///     Retrieves the product from the Trek web services and saves it to a file. If the
        ///     requested file is already present on the file system, then it is loaded instead.
        /// </summary>
        public void GetProduct(TerrainModelProductMetadata productInfo, Action<string> callback) {
            GetProduct(productInfo, false, callback);
        }

        /// <summary>
        ///     Retrieves the product from the Trek web services and saves it to a file. If the
        ///     requested file is already present on the file system, then it is loaded instead,
        ///     unless file redownload is forced.
        /// </summary>
        public void GetProduct(TerrainModelProductMetadata productInfo, bool forceRedownload, Action<string> callback) {

            if (productInfo.Format == 0) {
                productInfo.Format = ImageFileFormat.Tiff;
            }

            string filename = $"{productInfo.EncodeBase64()}.{FilePath.ProductFileExtension}";

            string directory = Path.Combine(FilePath.PersistentRoot, FilePath.Product);
            IList<string> availableFiles = FileUtils.ListFiles(directory, $"*.{FilePath.ProductFileExtension}", true);

            string filepath = Path.Combine(directory, filename);

            if (availableFiles.Contains(filename)) {

                if (!forceRedownload) {
                    callback(filepath);
                    return;
                }

                File.Delete(filepath);
            }

            string baseUrlWithFormat = $"{BaseUrl}/{productInfo.Format.FileExtension()}/subset";

            IDictionary<string, string> paramsMap = new Dictionary<string, string>() {
                { "itemUUID", productInfo.ProductId },
                { "bbox", productInfo.BoundingBox.ToString(",") },
                { "width", $"{productInfo.Width}" },
                { "height", $"{productInfo.Height}" }
            };

            string resourceUrl = HttpRequestUtils.AppendParams(baseUrlWithFormat, paramsMap, true);
            Debug.Log(resourceUrl);

            _httpClient.DownloadFile(resourceUrl, filepath, callback);

        }

    }

}
