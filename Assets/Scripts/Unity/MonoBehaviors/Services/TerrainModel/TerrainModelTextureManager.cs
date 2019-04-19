using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using static TrekVRApplication.BoundingBoxUtils;
using static TrekVRApplication.ServiceManager;
using static TrekVRApplication.TerrainConstants;
using Debug = UnityEngine.Debug;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public sealed class TerrainModelTextureManager : MonoBehaviourWithTaskQueue {

        private const int MaxGlobalTextures = 15;

        /// <summary>
        ///     The instance of the TerrainModelTextureManager that is present in the scene.
        ///     There should only be one TerrainModelTextureManager in the entire scene.
        /// </summary>
        public static TerrainModelTextureManager Instance { get; private set; }

        private class TextureWrapper {

            public readonly LinkedList<Action<Texture2D>> OnTextureLoad = new LinkedList<Action<Texture2D>>();

            private Texture2D _texture;
            public Texture2D Texture {
                get => _texture;
                set {
                    _texture = value;
                    if (OnTextureLoad.Count > 0) {
                        foreach (Action<Texture2D> action in OnTextureLoad) {
                            action.Invoke(_texture);
                        }
                        OnTextureLoad.Clear();
                    }
                }
            }

            public int UsageCount { get; set; } = 0;

            public long Created { get; } = DateTimeUtils.Now();

            public long LastUsed { get; set; } = 0;

            public static bool operator true(TextureWrapper o) {
                return o != null;
            }
            public static bool operator false(TextureWrapper o) {
                return o == null;
            }
            public static bool operator !(TextureWrapper o) {
                return o ? false : true;
            }
        }

        private readonly IDictionary<TerrainProductMetadata, TextureWrapper> _textureDictionary =
            new Dictionary<TerrainProductMetadata, TextureWrapper>();

        [SerializeField]
        private string _globalMosaicFilepath;

        private Texture2D _globalMosaicTexture;

        private LinkedList<Action<Texture2D>> _onGlobalMosaicLoad = new LinkedList<Action<Texture2D>>();

        #region Unity lifecycle methods

        public TerrainModelTextureManager() {
            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                throw new Exception($"Only one instance of {GetType().Name} is allowed.");
            }
        }

        private void Awake() {
            if (Instance != this) {
                Destroy(this);
            }

            // Load the global mosaic texture.
            string fullMosaicFilepath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.Product,
                _globalMosaicFilepath
            );
            LoadTextureFromImages(new string[] { fullMosaicFilepath }, texture => {
                _globalMosaicTexture = texture;
                foreach (Action<Texture2D> action in _onGlobalMosaicLoad) {
                    action.Invoke(_globalMosaicTexture);
                }
                _onGlobalMosaicLoad = null; // Set to null since it won't be used anymore after this.
            });

        }

        protected override void Update() {
            base.Update();
        }

        #endregion

        public void RegisterUsage(TerrainProductMetadata texInfo, bool inUse) {
            if (!_textureDictionary.TryGetValue(texInfo, out TextureWrapper wrapper)) {
                return;
            }
            if (inUse) {
                wrapper.UsageCount += 1;
                wrapper.LastUsed = DateTimeUtils.Now();
            } else {
                wrapper.UsageCount = MathUtils.Clamp(wrapper.UsageCount - 1, 0);
                if (wrapper.UsageCount == 0) {
                    ClearExcessTextures();
                }
            }
            PrintTextureList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">
        ///     A callback function that the requested Texture2D object will be passed through.
        ///     The callback function is guaranteed to be called on the main thread.
        /// </param>
        public void GetTexture(TerrainProductMetadata productInfo, Action<Texture2D> callback = null) {

            if (IsGlobalMosaic(productInfo)) {
                GetGlobalMosaicTexture(callback);
                return;
            }

            // If support for file formats other than TIFF are added in the future,
            // then this line will need to be removed to retrieve specific formats.
            productInfo = CleanMetadata(productInfo);

            // If an entry for the texture doesn't exist yet, then create it.
            if (!_textureDictionary.TryGetValue(productInfo, out TextureWrapper wrapper)) {
                _textureDictionary.Add(productInfo, wrapper = new TextureWrapper());

                // If the bounding box crosses the +/- 180° longitude line, then the
                // texture will need to be retrieved in two parts and then merged.
                if (IsLongitudeWrapped(productInfo.BoundingBox)) {

                    string filepath1 = null;
                    string filepath2 = null;

                    // Get the first half of the texture.
                    RasterSubsetWebService.SubsetProduct(UnwrapBoundingBoxLeft(productInfo), filepath => {
                        filepath1 = filepath;

                        // If the second image was already finished downloading,
                        // then convert the two images into a texture.
                        if (filepath2 != null) {
                            LoadTextureFromImages(new string[] { filepath1, filepath2 }, texture => {
                                wrapper.Texture = texture;
                                ClearExcessTextures();
                            });
                        }
                    });

                    // Get the second half of the texture.
                    RasterSubsetWebService.SubsetProduct(UnwrapBoundingBoxRight(productInfo), filepath => {
                        filepath2 = filepath;

                        // If the first image was already finished downloading,
                        // then convert the two images into a texture.
                        if (filepath1 != null) {
                            LoadTextureFromImages(new string[] { filepath1, filepath2 }, texture => {
                                wrapper.Texture = texture;
                                ClearExcessTextures();
                            });
                        }
                    });

                }

                // Else, the texture can be retrieved and processed as a whole.
                else {
                    RasterSubsetWebService.SubsetProduct(productInfo, filepath => {
                        LoadTextureFromImages(new string[] { filepath }, texture => {
                            wrapper.Texture = texture;
                            ClearExcessTextures();
                        });
                    });
                }

            }

            // Register the callback function if it is not null.
            if (callback != null) {
                if (!wrapper.Texture) {
                    wrapper.OnTextureLoad.AddLast(callback);
                } else {
                    QueueTask(() => {
                        callback(wrapper.Texture);
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">
        ///     A callback function that the requested Texture2D object will be passed through.
        ///     The callback function is guaranteed to be called on the main thread.
        /// </param>
        public void GetGlobalMosaicTexture(Action<Texture2D> callback = null) {
            if (!_globalMosaicTexture) {
                _onGlobalMosaicLoad.AddLast(callback);
            } else {
                QueueTask(() => {
                    callback(_globalMosaicTexture);
                });
            }
        }

        /// <summary>
        ///     Prints debug info to the console.
        /// </summary>
        public void PrintTextureList() {
            foreach (TerrainProductMetadata texInfo in _textureDictionary.Keys) {
                TextureWrapper wrapper = _textureDictionary[texInfo];
                Debug.Log(texInfo + "\n" + wrapper.UsageCount);
            }
        }

        private TerrainProductMetadata CleanMetadata(TerrainProductMetadata texInfo) {
            return new TerrainProductMetadata(
                texInfo.ProductUUID,
                texInfo.BoundingBox,
                texInfo.Width,
                texInfo.Height
            );
        }

        private TerrainProductMetadata UnwrapBoundingBoxLeft(TerrainProductMetadata texInfo) {
            return new TerrainProductMetadata(
                texInfo.ProductUUID,
                UnwrapLeft(texInfo.BoundingBox),
                texInfo.Width,
                texInfo.Height
            );
        }

        private TerrainProductMetadata UnwrapBoundingBoxRight(TerrainProductMetadata texInfo) {
            return new TerrainProductMetadata(
                texInfo.ProductUUID,
                UnwrapRight(texInfo.BoundingBox),
                texInfo.Width,
                texInfo.Height
            );
        }

        private void LoadTextureFromImages(string[] filepaths, Action<Texture2D> callback) {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Create and execute task to convert first image into an RGBImage object.
            new LoadColorImageFromFileTask<RGBImage>(filepaths[0]).Execute(baseImage => {

                // If there are more files to process, then load and merge each image.
                for (int i = 1; i < filepaths.Length; i++) {

                    // Load the additional image. The additional images can be loaded 
                    // syncronously in the same thread as this anonymous function.
                    RGBImage image = new LoadColorImageFromFileTask<RGBImage>(filepaths[i]).ExecuteInCurrentThread();

                    // Merge the images together.
                    try {
                        baseImage.Merge(image, new Color32(0, 0, 0, 255));
                    }
                    catch (Exception e) {
                        Debug.LogError(e.Message);
                        continue;
                    }
                }

                int width = baseImage.Width;
                int height = baseImage.Height;

                TextureCompressionFormat format = TextureCompressionFormat.Uncompressed;

                byte[] data = new byte[TextureUtils.ComputeTextureSize(width, height, format)];
                baseImage.CopyRawData(data);

                Debug.Log($"Took {stopwatch.ElapsedMilliseconds}ms to generate texture.");
                stopwatch.Stop();

                QueueTask(() => {
                    Texture2D texture = new Texture2D(width, height, format.GetUnityFormat(), true);
                    texture.GetRawTextureData<byte>().CopyFrom(data);
                    texture.Apply(true, true);
                    callback?.Invoke(texture);
                });

            });
        }

        private bool IsGlobalMosaic(TerrainProductMetadata texInfo) {
            return texInfo.ProductUUID == GlobalMosaicUUID && texInfo.BoundingBox == UnrestrictedBoundingBox.Global;
        }

        private int ClearExcessTextures() {

            // TODO Test this

            int targetDeleteCount = _textureDictionary.Count - MaxGlobalTextures;
            if (targetDeleteCount < 0) {
                return 0;
            }

            IEnumerable<KeyValuePair<TerrainProductMetadata, TextureWrapper>> entries =
                _textureDictionary.ToList()
                .Where(kv => kv.Value.UsageCount == 0)
                .OrderByDescending(kv => kv.Value.LastUsed);

            int count = 0;
            foreach (KeyValuePair<TerrainProductMetadata, TextureWrapper> entry in entries) {
                _textureDictionary.Remove(entry.Key);
                Destroy(entry.Value.Texture);
                if (++count == targetDeleteCount) {
                    break;
                }
            }

            return count;
        }

    }

    

}
