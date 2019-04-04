using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TrekVRApplication.SearchResponse;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Implementation of IProductWebService that retrieves products from the
    ///     Trek web services.
    /// </summary>
    public class TrekRasterSubsetWebService : IRasterSubsetWebService {

        public static TrekRasterSubsetWebService Instance { get; } = new TrekRasterSubsetWebService();

        private const string BaseUrl = "https://trek.nasa.gov/marsbeta/TrekServices/ws";

        private const string SubsetUrl = "/raster";

        private const string SearchUrl = "/index/eq/searchRaster";

        private HttpClient _httpClient = HttpClient.Instance;

        private SearchResult _rasters;

        private TrekRasterSubsetWebService() {

        }

        public void ClearCache() {
            _rasters = null;
        }

        public void GetRasters(Action<SearchResult> callback, bool forceRefresh = false) {
            if (forceRefresh) {
                _rasters = null;
            }
            if (_rasters) {
                callback?.Invoke(_rasters);
                return;
            }
            IDictionary<string, string> paramsMap = new Dictionary<string, string>() {
                { "productType", "*" },
            };
            string searchUrl = HttpRequestUtils.AppendParams($"{BaseUrl}{SearchUrl}", paramsMap);
            _httpClient.Get(searchUrl, (res) => {
                string responseBody = HttpClient.GetReponseBody(res);
                _rasters = DeserializeResults(responseBody);
                callback?.Invoke(_rasters);
            });
        }

        /// <summary>
        ///     Retrieves the product from the Trek web services and saves it to a file. If the
        ///     requested file is already present on the file system, then it is loaded instead.
        /// </summary>
        public void SubsetProduct(TerrainModelProductMetadata productInfo, Action<string> callback) {
            SubsetProduct(productInfo, false, callback);
        }

        /// <summary>
        ///     Retrieves the product from the Trek web services and saves it to a file. If the
        ///     requested file is already present on the file system, then it is loaded instead,
        ///     unless file redownload is forced.
        /// </summary>
        public void SubsetProduct(TerrainModelProductMetadata productInfo, bool forceRedownload, Action<string> callback) {

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

            string baseUrlWithFormat = $"{BaseUrl}{SubsetUrl}/{productInfo.Format.FileExtension()}/subset";

            IDictionary<string, string> paramsMap = new Dictionary<string, string>() {
                { "itemUUID", productInfo.ProductUUID },
                { "bbox", productInfo.BoundingBox.ToString(",") },
                { "width", $"{productInfo.Width}" },
                { "height", $"{productInfo.Height}" }
            };

            string resourceUrl = HttpRequestUtils.AppendParams(baseUrlWithFormat, paramsMap, true);
            Debug.Log(resourceUrl);

            VerifyProductExists(productInfo, exists => {
                if (exists) {
                    _httpClient.DownloadFile(resourceUrl, filepath, callback);
                } else {
                    Debug.LogError($"Product UUID {productInfo.ProductUUID} is not a raster or does not exist.");
                }
            });

        }

        private void VerifyProductExists(TerrainModelProductMetadata productInfo, Action<bool> callback) {
            GetRasters(res => {
                foreach (SearchResultItem item in res.Items) {
                    if (item.UUID == productInfo.ProductUUID) {
                        callback.Invoke(true);
                        return;
                    }
                }
                callback.Invoke(false);
            });
        }

        private SearchResult DeserializeResults(string json) {
            Result result = JsonConvert.DeserializeObject<Result>(json, JsonConfig.SerializerSettings);
            return new SearchResult(result);
        }

    }

}
