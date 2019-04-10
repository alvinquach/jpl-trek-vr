using System.Collections.Generic;
using UnityEngine;
using static TrekVRApplication.FlagUtils;
using static TrekVRApplication.TerrainModelConstants;
using static TrekVRApplication.ServiceManager;
using System;

namespace TrekVRApplication {

    public abstract class TerrainModelLayerController : MonoBehaviour {

        /// <summary>
        ///     The maximum number of diffuese layers, including the base layer.
        /// </summary>
        private const int MaxDiffuseLayers = 8;

        protected readonly TerrainModelManager _terrainModelManager = TerrainModelManager.Instance;

        protected bool Started { get; private set; }

        public abstract IList<TerrainModelLayer> Layers { get; }

        private Material _material;
        public Material Material {
            get {
                if (!_material) {
                    _material = GenerateMaterial();
                }
                return _material;
            }
            set {
                if (Started) {
                    Debug.LogError("Material cannot be set externally after Start() has been called.");
                    return;
                }
                _material = new Material(value);
            }
        }

        private IBoundingBox _boundingBox = UnrestrictedBoundingBox.Global;
        public IBoundingBox BoundingBox {
            get => _boundingBox;
            set {
                if (Started) {
                    Debug.LogError("Bounding box cannot be directly set after Start() has been called. Use UpdateBoundingBox() instead.");
                    return;
                }
                _boundingBox = value;
            }
        }

        public Vector2Int TargetTextureSize { get; set; } = Vector2Int.one * LocalTerrainTextureTargetSize;

        #region Render mode properties

        public bool EnableOverlay {
            get => ContainsFlag((int)RenderMode, (int)TerrainModelRenderMode.Overlay);
            set => RenderMode = (TerrainModelRenderMode)AddOrRemoveFlag((int)RenderMode, (int)TerrainModelRenderMode.Overlay, value);
        }

        public bool DisableTextures {
            get => ContainsFlag((int)RenderMode, (int)TerrainModelRenderMode.NoTextures);
            set => RenderMode = (TerrainModelRenderMode)AddOrRemoveFlag((int)RenderMode, (int)TerrainModelRenderMode.NoTextures, value);
        }

        public bool UseDisabledMaterial {
            get => ContainsFlag((int)RenderMode, (int)TerrainModelRenderMode.Disabled);
            set => RenderMode = (TerrainModelRenderMode)AddOrRemoveFlag((int)RenderMode, (int)TerrainModelRenderMode.Disabled, value);
        }

        private TerrainModelRenderMode _renderMode;
        public TerrainModelRenderMode RenderMode {
            get => _renderMode;
            private set {
                _renderMode = value;
                UpdateMaterialProperties();
            }
        }

        #endregion

        #region Unity lifecycle methods

        protected virtual void Start() {
            Started = true;
            UseDisabledMaterial = !_terrainModelManager.TerrainInteractionEnabled;
            DisableTextures = !_terrainModelManager.TerrainTexturesEnabled;
            ReloadTextures(Layers);
        }

        protected virtual void OnDestroy() {
            Destroy(Material);
            // TODO Destroy more things
        }

        #endregion

        public void AddLayer(string productUUID, Action callback) {
            AddLayer(productUUID, callback, null);
        }

        public void AddLayer(string productUUID, int index) {
            AddLayer(productUUID, null, index);
        }

        public abstract void AddLayer(string productUUID, Action callback = null, int? index = null);

        public abstract void UpdateLayer(TerrainModelLayerChange changes, Action callback = null);

        public abstract void RemoveLayer(int index, Action callback = null);

        /// <summary>
        ///     Updates the bounding box of the terrain model. The layer controller
        ///     will reload the textures accordingly.
        /// </summary>
        /// <param name="bbox">The new bounding box.</param>
        /// <param name="useTemporaryTextures">
        ///     Whether to use existing textures to the mesh as a placeholder until  
        ///     the more detailed textures can be loaded.
        /// </param>
        public void UpdateBoundingBox(IBoundingBox bbox, bool useTemporaryTextures = true) {
            if (bbox == BoundingBox) {
                return;
            }

            if (useTemporaryTextures) {
                UVScaleOffset uvScaleOffset = BoundingBoxUtils.CalculateUVScaleOffset(BoundingBox, bbox);
                for (int i = 0; i < MaxDiffuseLayers; i++) {
                    int layerId = GetShaderTextureId(i);
                    if (!Material.GetTexture(layerId)) {
                        continue;
                    }
                    Material.SetTextureScale(layerId, uvScaleOffset.Scale);
                    Material.SetTextureOffset(layerId, uvScaleOffset.Offset);
                }
            }
            else {
                for (int i = 0; i < MaxDiffuseLayers; i++) {
                    int layerId = GetShaderTextureId(i);
                    if (Material.GetTexture(layerId)) {
                        Material.SetTexture(layerId, null);
                    }
                }
            }

            BoundingBox = bbox;

            if (Started) {
                ReloadTextures(Layers);
            }
        }

        #region Render mode methods

        /// <summary>
        ///     Update the material properties to reflect current render mode.
        /// </summary>
        public void UpdateMaterialProperties() {

            Debug.Log($"DisableTextures={DisableTextures}, UseDisabledMaterial ={UseDisabledMaterial}, EnableOverlay={EnableOverlay}");

            string shaderName;
            if (DisableTextures) {
                shaderName = UseDisabledMaterial ? "NoTexturesOverlayTransparent" : "NoTexturesOverlay";
            }
            else {
                shaderName = UseDisabledMaterial ? "MultiDiffuseOverlayTransparent" : "MultiDiffuseOverlay";
            }

            SwitchToShader($"Custom/Terrain/{shaderName}");

            // This is redundant, since if UseDisabledMaterial is false, the shader
            // is not used and the _DiffuseOpacity parameter has no effect.
            Material.SetFloat("_DiffuseOpacity", UseDisabledMaterial ? 0.5f : 1);   // TODO Define the opacity as a constant.

            Material.SetFloat("_Glossiness", DisableTextures ? NoTextureShaderSmoothness : ShaderSmoothness);
            Material.SetFloat("_Metallic", DisableTextures ? NoTextureShaderMetallic : ShaderMetallic);

            Material.SetFloat("_OverlayOpacity", EnableOverlay ? 1 : 0);
        }

        /// <summary>
        ///     Helper method for switching between different shaders.
        /// </summary>
        private void SwitchToShader(string shaderName) {
            Shader shader = Shader.Find(shaderName);
            if (shader) {
                Material.shader = shader;
            }
            else {
                Debug.LogError($"Could not find shader {shaderName}.");
            }
        }

        #endregion

        protected abstract Material GenerateMaterial();

        protected void ReloadTextures(IList<TerrainModelLayer> layers) {
            TerrainModelTextureManager textureManager = TerrainModelTextureManager.Instance;
            for (int i = 0; i < MaxDiffuseLayers; i++) {
                int layerId = GetShaderTextureId(i);
                if (i < layers.Count) {
                    textureManager.GetTexture(GenerateProductMetadata(layers[i].ProductUUID), texture => {
                        Material.SetTexture(layerId, texture);
                        Material.SetTextureScale(layerId, Vector2.one);
                        Material.SetTextureOffset(layerId, Vector2.zero);
                    });
                }
                else {
                    Material.SetTexture(layerId, null);
                }
            }
        }

        protected void CreateLayer(string uuid, Action<TerrainModelLayer> callback) {
            RasterSubsetWebService.GetRaster(uuid, raster => {
                if (raster == null) {
                    throw new Exception("No result.");
                }
                callback(new TerrainModelLayer(raster.Name, uuid, true));
            });
        }

        protected TerrainModelProductMetadata GenerateProductMetadata(string uuid) {
            return new TerrainModelProductMetadata(uuid, BoundingBox, TargetTextureSize.x, TargetTextureSize.y);
        }

        protected int GetShaderTextureId(int index) {
            return Shader.PropertyToID(index == 0 ? "_DiffuseBase" : $"_Diffuse{index}");
        }

    }

}