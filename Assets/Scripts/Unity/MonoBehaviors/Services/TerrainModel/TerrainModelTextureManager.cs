using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public sealed class TerrainModelTextureManager : MonoBehaviourWithTaskQueue {

        private const int MaxGlobalTextures = 8;

        private class TextureWrapper {

            public readonly LinkedList<Action<Texture2D>> OnLoadTexture = new LinkedList<Action<Texture2D>>();

            private Texture2D _texture;
            public Texture2D Texture {
                get { return _texture; }
                set {
                    _texture = value;
                    if (OnLoadTexture.Count > 0) {
                        foreach (Action<Texture2D> action in OnLoadTexture) {
                            action.Invoke(_texture);
                        }
                        OnLoadTexture.Clear();
                        LastUsed = DateTimeUtils.Now();
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

        private readonly IProductWebService _productWebService = TrekProductWebService.Instance;

        private readonly IDictionary<TerrainModelProductMetadata, TextureWrapper> _textureDictionary =
            new Dictionary<TerrainModelProductMetadata, TextureWrapper>();

        protected override void Update() {
            base.Update();
        }

        public void RegisterUsage(TerrainModelProductMetadata texInfo, bool inUse) {
            TextureWrapper textureWrapper = _textureDictionary[texInfo];
            if (!textureWrapper) {

                return;
            }
            textureWrapper.UsageCount += MathUtils.Clamp(textureWrapper.UsageCount + (inUse ? 1 : -1), 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">
        ///     A callback function that the requested Texture2D object will be passed through.
        ///     The callback function is guaranteed to be called on the main thread.
        /// </param>
        public void GetTexture(TerrainModelProductMetadata productInfo, Action<Texture2D> callback = null) {

            TerrainModelProductMetadata texInfo = CleanMetadata(productInfo);

            TextureWrapper wrapper = _textureDictionary[texInfo];

            // If an entry for the texture doesn't exist yet, then create it.
            if (!wrapper) {

                _textureDictionary[texInfo] = wrapper = new TextureWrapper();

                // Get product either from the file system (if available) or the web service.
                _productWebService.GetProduct(productInfo, filepath => {

                    // Create a task for loading texture data on a separate thread.
                    LoadColorImageFromFileTask<BGRAImage> loadImageTask = new LoadColorImageFromFileTask<BGRAImage>(filepath);

                    // Execute the task.
                    loadImageTask.Execute(image => {

                        int width = loadImageTask.TextureWidth;
                        int height = loadImageTask.TextureHeight;

                        TextureCompressionFormat format = TextureCompressionFormat.Uncompressed;

                        byte[] data = new byte[TextureUtils.ComputeTextureSize(width, height, format)];
                        image.CopyRawData(data);

                        QueueTask(() => {
                            Texture2D texture = new Texture2D(width, height, format.GetUnityFormat(), true);
                            texture.GetRawTextureData<byte>().CopyFrom(data);
                            texture.Apply(true);
                            wrapper.Texture = texture;
                        });

                    });
                });
            }

            // Register the callback function if it is not null.
            if (callback != null) {
                if (!wrapper.Texture) {
                    wrapper.OnLoadTexture.AddLast(callback);
                } else {
                    QueueTask(() => {
                        callback(wrapper.Texture);
                        wrapper.LastUsed = DateTimeUtils.Now();
                    });
                }
            }
        }

        private TerrainModelProductMetadata CleanMetadata(TerrainModelProductMetadata texInfo) {
            return new TerrainModelProductMetadata(
                texInfo.ProductId,
                texInfo.BoundingBox,
                texInfo.Width,
                texInfo.Height
            );
        }

    }

    

}
