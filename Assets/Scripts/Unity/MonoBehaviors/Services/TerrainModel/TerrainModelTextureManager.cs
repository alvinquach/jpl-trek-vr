using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;
using System.Linq;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public sealed class TerrainModelTextureManager : MonoBehaviourWithTaskQueue {

        private const int MaxGlobalTextures = 8;

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

        private readonly IRasterSubsetWebService _productWebService = TrekRasterSubsetWebService.Instance;

        private readonly IDictionary<TerrainModelProductMetadata, TextureWrapper> _textureDictionary =
            new Dictionary<TerrainModelProductMetadata, TextureWrapper>();

        [SerializeField]
        private string _globalMosaicFilepath;

        private Texture2D _globalMosaicTexture;

        private LinkedList<Action<Texture2D>> _onGlobalMosaicLoad = new LinkedList<Action<Texture2D>>();

        #region Unity lifecycle methods

        public TerrainModelTextureManager() {
            if (!Instance) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this);
                throw new Exception($"Only one instance of {GetType().Name} is allowed.");
            }
        }

        private void Awake() {

            // Load the global mosaic texture.
            string fullMosaicFilepath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.Product,
                _globalMosaicFilepath
            );
            LoadTextureFromImage(fullMosaicFilepath, texture => {
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

        public void RegisterUsage(TerrainModelProductMetadata texInfo, bool inUse) {
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
        public void GetTexture(TerrainModelProductMetadata productInfo, Action<Texture2D> callback = null) {

            if (IsGlobalMosaic(productInfo)) {
                GetGlobalMosaicTexture(callback);
                return;
            }

            TerrainModelProductMetadata texInfo = CleanMetadata(productInfo);

            // If an entry for the texture doesn't exist yet, then create it.
            if (!_textureDictionary.TryGetValue(texInfo, out TextureWrapper wrapper)) {
                _textureDictionary.Add(texInfo, wrapper = new TextureWrapper());

                // Get product either from the file system (if available) or the web service.
                _productWebService.SubsetProduct(productInfo, filepath => {
                    LoadTextureFromImage(filepath, texture => {
                        wrapper.Texture = texture;
                        ClearExcessTextures();
                    });
                });
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
            foreach (TerrainModelProductMetadata texInfo in _textureDictionary.Keys) {
                TextureWrapper wrapper = _textureDictionary[texInfo];
                Debug.Log(texInfo + "\n" + wrapper.UsageCount);
            }
        }

        private TerrainModelProductMetadata CleanMetadata(TerrainModelProductMetadata texInfo) {
            return new TerrainModelProductMetadata(
                texInfo.ProductUUID,
                texInfo.BoundingBox,
                texInfo.Width,
                texInfo.Height
            );
        }

        private void LoadTextureFromImage(string filepath, Action<Texture2D> callback) {

            long start = DateTimeUtils.Now();

            // Create a task for loading texture data on a separate thread.
            LoadColorImageFromFileTask<RGBImage> loadImageTask = new LoadColorImageFromFileTask<RGBImage>(filepath);

            // Execute the task.
            loadImageTask.Execute(image => {

                int width = loadImageTask.TextureWidth;
                int height = loadImageTask.TextureHeight;

                TextureCompressionFormat format = TextureCompressionFormat.Uncompressed;

                byte[] data = new byte[TextureUtils.ComputeTextureSize(width, height, format)];
                image.CopyRawData(data);
                Debug.Log($"Took {(DateTimeUtils.Now() - start) / 1000f} seconds to generate texture.");

                QueueTask(() => {
                    Texture2D texture = new Texture2D(width, height, format.GetUnityFormat(), true);
                    texture.GetRawTextureData<byte>().CopyFrom(data);
                    texture.Apply(true, true);
                    callback?.Invoke(texture);
                });

            });
        }

        private bool IsGlobalMosaic(TerrainModelProductMetadata texInfo) {
            return texInfo.ProductUUID == GlobalMosaicUUID && texInfo.BoundingBox == UnrestrictedBoundingBox.Global;
        }

        private int ClearExcessTextures() {

            // TODO Test this

            int targetDeleteCount = _textureDictionary.Count - MaxGlobalTextures;
            if (targetDeleteCount < 0) {
                return 0;
            }

            IEnumerable<KeyValuePair<TerrainModelProductMetadata, TextureWrapper>> entries =
                _textureDictionary.ToList()
                .Where(kv => kv.Value.UsageCount == 0)
                .OrderByDescending(kv => kv.Value.LastUsed);

            int count = 0;
            foreach (KeyValuePair<TerrainModelProductMetadata, TextureWrapper> entry in entries) {
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
