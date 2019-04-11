using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TrekVRApplication.TerrainConstants;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class TerrainModelManager : SingletonMonoBehaviour<TerrainModelManager> {

        /// <summary>
        ///     The maximum number of models that this controller can manage,
        ///     excluding the default planet model. Once the number of models
        ///     reaches this maximum, the oldest model will have to be deleted
        ///     when a new one is created.
        /// </summary>
        private const int MaxModels = 5;

        private int _modelCounter = 0;

        /// <summary>
        ///     The material that is used as a base for new terrain models.
        /// </summary>
        public Material BaseMaterial { get; private set; }

        #region Globe model fields/properties.

        // TODO Change this to a const.
        [SerializeField]
        private string _globalDEMFilepath;
        public string GlobalDEMFilepath => _globalDEMFilepath;

        [SerializeField]
        private int _globeModelBaseDownsampleLevel = 1;

        [SerializeField]
        private int _globeModelLODLevels = 2;

        public GlobeTerrainModel GlobeModel { get; private set; }

        #endregion

        // TODO Find out how the initial capacity for lists works in C#.
        private List<TerrainModel> _terrainModels = new List<TerrainModel>();

        private GameObject _terrainModelsContainer;

        private TerrainModel _currentVisibleModel;
        public TerrainModel CurrentVisibleModel {
            get => _currentVisibleModel;
            private set {
                if (_currentVisibleModel != value) {
                    _currentVisibleModel = value;
                    OnCurrentTerrainModelChange.Invoke(value);
                }
            }
        }

        #region Globally shared properites

        // NOTE: When modifying the global layers list within this class,
        // access the backing variable directly instead of the property,
        // as using the property accessor will return a new list object.
        private readonly IList<TerrainLayer> _globalLayers = new List<TerrainLayer>();
        /// <summary>
        ///     Get a copy of the global layer list.
        /// </summary>
        public IList<TerrainLayer> GlobalLayers => new List<TerrainLayer>(_globalLayers);

        private bool _terrainInteractionEnabled = true;
        /// <summary>
        ///     Whether the terrain models are set to render in opaque (enabled) mode
        ///     or semi-transparent (disabled) mode.
        /// </summary>
        public bool TerrainInteractionEnabled {
            get => _terrainInteractionEnabled;
            set {
                if (_terrainInteractionEnabled != value) {
                    _terrainInteractionEnabled = value;
                    OnEnableTerrainInteractionChange.Invoke(value);
                }
            }
        }

        private bool _terrainTexturesEnabled = true;
        public bool TerrainTexturesEnabled {
            get => _terrainTexturesEnabled;
            set {
                if (_terrainTexturesEnabled != value) {
                    _terrainTexturesEnabled = value;
                    OnEnableTerrainTexturesChange.Invoke(value);
                }
            }
        }

        private float _heightExagerration = 1.0f;
        /// <summary>
        ///     Terrain height exaggeration.
        /// </summary>
        public float HeightExagerration {
            get => _heightExagerration;
            set {
                if (_heightExagerration != value) {
                    _heightExagerration = value;
                    Debug.Log($"Terrain height exaggeration set to {value}.");
                    OnHeightExagerrationChange.Invoke(value);
                }
            }
        }

        #endregion

        #region Event emitters

        public event Action<bool> OnEnableTerrainInteractionChange = e => { };

        public event Action<bool> OnEnableTerrainTexturesChange = e => { };

        public event Action<float> OnHeightExagerrationChange = e => { };

        public event Action OnCurrentModelPhyscisMeshUpdated = () => { };

        public event Action<TerrainModel> OnCurrentTerrainModelChange = e => { };

        public event Action OnGlobalLayersChanged = () => { };

        public void ReportPhysicsMeshUpdated(TerrainModel terrainModel) {
            if (terrainModel != CurrentVisibleModel) {
                return;
            }
            OnCurrentModelPhyscisMeshUpdated.Invoke();
        }

        #endregion

        #region Unity lifecycle methods

        protected override void Awake() {
            base.Awake();

            // Create a game object that will contain all the terrain model game objects.
            _terrainModelsContainer = new GameObject(GameObjectName.TerrainModelsContainer) {
                layer = (int)CullingLayer.Terrain
            };

            // Create base material.
            InitializeMaterial();

            // Create the globe model.
            InitializeGlobeTerrainModel();
            CurrentVisibleModel = GlobeModel;

            // Shift up 1 meter.
            _terrainModelsContainer.transform.position = Vector3.up;

        }

        #endregion

        #region Globe highlight area
        // TODO Move the variables and methods in this region elsewhere.

        private TerrainOverlayArea _highlightedArea;

        public void HighlightAreaOnGlobe(BoundingBox bbox) {
            if (!_highlightedArea) {
                _highlightedArea = 
                    GlobeTerrainOverlayController.Instance.AddArea(new Color(0.0f, 0.9f, 1.0f, 0.5f));
            }
            _highlightedArea.UpdateArea(bbox);
        }

        public void ClearHighlightedAreaOnGlobe() {
            GlobeTerrainOverlayController.Instance.RemoveObject(_highlightedArea);
            _highlightedArea = null;
        }

        #endregion

        #region Layer management methods

        public bool AddGlobalLayer(TerrainLayer layer, int? index = null) {
            if (_globalLayers.Any(l => l.ProductUUID == layer.ProductUUID)) {
                Debug.LogError($"{layer.ProductUUID} has already been added as a layer.");
                return false;
            }
            if (index == null) {
                _globalLayers.Add(layer);
            }
            else {
                _globalLayers.Insert((int)index, layer);
            }
            OnGlobalLayersChanged.Invoke();
            return true;
        }

        public bool UpdateGlobalLayer(TerrainLayerChange changes) {
            int index = changes.Index;
            if (index <= 0 || index >= _globalLayers.Count) {
                return false;
            }
            bool changed = false;
            TerrainLayer layer = _globalLayers[index];
            if (changes.Opacity != null && layer.Opacity != changes.Opacity) {
                layer.Opacity = (float)changes.Opacity;
                changed = true;
            }
            if (changes.Visible != null && layer.Visible != changes.Visible) {
                layer.Visible = (bool)changes.Visible;
                changed = true;
            }
            _globalLayers[index] = layer;
            OnGlobalLayersChanged.Invoke();
            return changed;
        }

        public bool MoveGlobalLayer(int from, int to) {
            if (from < 0 || from >= _globalLayers.Count || from < 0 || from >= _globalLayers.Count || from == to) {
                return false;
            }
            TerrainLayer temp = _globalLayers[from];
            _globalLayers.RemoveAt(from);
            _globalLayers.Insert(to, temp);
            OnGlobalLayersChanged.Invoke();
            return true;
        }

        public bool RemoveGlobalLayer(int index) {
            if (index < 0 || index >= _globalLayers.Count) {
                return false;
            }
            _globalLayers.RemoveAt(index);
            OnGlobalLayersChanged.Invoke();
            return true;
        }

        #endregion

        #region Model creation methods

        public LocalTerrainModel CreateLocalModelFromSubset(TerrainModel parent, BoundingBox boundingBox) {

            LocalTerrainModel terrainModel = CreateLocalModel(boundingBox, parent.DemUUID, parent is GlobeTerrainModel);

            // TODO Make this code look better
            TerrainLayerController layerController;
            TerrainLayerController parentLayerController = parent.LayerController;
            if (parentLayerController is GlobalTerrainLayerController) {
                layerController = terrainModel.AddLayerController<GlobalTerrainLayerController>();
            } else {
                layerController = terrainModel.AddLayerController<TerrainBookmarkLayerController>();
                int index = 0;
                foreach (TerrainLayer layer in parentLayerController.Layers) {
                    TerrainLayerChange changes = new TerrainLayerChange() {
                        Index = index,
                        Opacity = layer.Opacity,
                        Visible = layer.Visible
                    };
                    layerController.AddLayer(layer.ProductUUID, () => {
                        layerController.UpdateLayer(changes);
                    });
                    index++;
                }
            }

            layerController.Material = parentLayerController.Material;

            // Set the bounding box to the parent's bounding box first,
            // and then call update bounding box so that the parent's
            // textures can be temporarily used correctly.
            layerController.BoundingBox = parentLayerController.BoundingBox;
            layerController.UpdateBoundingBox(terrainModel.SquareBoundingBox);

            return terrainModel;
        }

        public LocalTerrainModel CreateLocalModelFromBookmark(BoundingBox boundingBox, string demUUID, IList<string> layersUUID) {

            LocalTerrainModel terrainModel = CreateLocalModel(boundingBox, demUUID, false, false);

            TerrainLayerController layerController = terrainModel.AddLayerController<TerrainBookmarkLayerController>();
            layerController.UpdateBoundingBox(terrainModel.SquareBoundingBox, false);
            foreach(string layerUUID in layersUUID) {
                layerController.AddLayer(layerUUID);
            }

            return terrainModel;
        }

        private LocalTerrainModel CreateLocalModel(BoundingBox boundingBox, string demUUID, 
            bool initWithAnimations = true, bool useTemporaryBaseTextures = true) {

            GameObject terrainModelContainer = new GameObject($"Model {++_modelCounter}") {
                layer = (int)CullingLayer.Terrain
            };
            terrainModelContainer.transform.SetParent(_terrainModelsContainer.transform, false);
            terrainModelContainer.SetActive(false);

            LocalTerrainModel terrainModel = terrainModelContainer.AddComponent<LocalTerrainModel>();
            terrainModel.Radius = Mars.Radius;
            terrainModel.DemUUID = demUUID;
            terrainModel.UseTemporaryBaseTextures = useTemporaryBaseTextures;
            terrainModel.BoundingBox = boundingBox;
            terrainModel.LodLevels = 0;
            terrainModel.PhysicsDownsampleLevel = LocalTerrainPhysicsTargetDownsample;
            terrainModel.AnimateOnInitialization = initWithAnimations;
            return (LocalTerrainModel)AddTerrainModel(terrainModel);
        }

        #endregion

        #region Model visiblity control methods

        /// <summary>
        ///     Whether the globe model is currently visible as per this controller. 
        /// </summary>
        public bool GlobeModelIsVisible() {
            return GlobeModel.Visible;
        }

        /// <summary>
        ///     Shows the globe model and hides the other TerrainModelBase objects that are
        ///     managed by this controller.
        /// </summary>
        public void ShowGlobeModel() {
            if (CurrentVisibleModel != GlobeModel) {
                _terrainModels.ForEach(t => t.Visible = false);
                CurrentVisibleModel = GlobeModel;
                GlobeModel.Visible = true;
            }
        }

        /// <summary>
        ///     Set the TerrainModelBase object to be visible by setting its GameObject to active.
        ///     Does not have any effect if the TerrainModelBase's GameObject was already active,
        ///     or if the TerrainModelBase is not managed by this controller.
        /// </summary>
        /// <returns>
        ///     True if the visiblity of the TerrainModel objects in this
        ///     controller was changed. Otherwise, returns false.
        /// </returns>
        // TODO Write a separate method to clone the rotation.
        public bool ShowTerrainModel(TerrainModel terrainModel, bool cloneRotation = false) {
            if (terrainModel == null || terrainModel.Visible) {
                return false; // Retun false if the terrain model is already visible.
            }
            HideAll();
            CurrentVisibleModel = terrainModel;
            terrainModel.Visible = true;
            if (cloneRotation) {
                terrainModel.transform.rotation = GlobeModel.transform.rotation;
            }
            return true;
        }

        /// <summary>
        ///     Hides all the TerrainModelBase objects managed by this controller by setting their
        ///     GameObjects to inactive. Includes the default planet model.
        /// </summary>
        public void HideAll() {
            CurrentVisibleModel = null;
            GlobeModel.Visible = false;
            _terrainModels.ForEach(w => w.Visible = false);
        }

        #endregion

        public T GetComponentFromCurrentModel<T>() {

            // FIXME Change this so that it actually gets the component
            // for the current model instead of the globe model.
            return GlobeModel.GetComponent<T>();
        }

        public Transform GetGlobeModelTransform() {
            return GlobeModel.transform;
        }

        private TerrainModel AddTerrainModel(TerrainModel terrainModel) {

            // If the number of terrain models being managed already exceeds the max limit,
            // then we have to remove one to make space for the new one. We will remove the
            // one with the oldest LastActive time.
            if (_terrainModels.Count >= MaxModels) {

                // TODO Test this
                TerrainModel oldest = _terrainModels.OrderByDescending(w => w.LastVisible).Last();

                Destroy(oldest.gameObject);
                _terrainModels.Remove(oldest);
            }

            _terrainModels.Add(terrainModel);
            return terrainModel;
        }

        #region Initialization methods

        private void InitializeGlobeTerrainModel() {
            GameObject globeModelGameObject = new GameObject(typeof(Mars).Name) {
                layer = (int)CullingLayer.Terrain
            };
            globeModelGameObject.transform.SetParent(_terrainModelsContainer.transform, false);
            GlobeModel = globeModelGameObject.AddComponent<GlobeTerrainModel>();
            GlobeModel.Radius = Mars.Radius;
            GlobeModel.BaseDownsampleLevel = _globeModelBaseDownsampleLevel;
            GlobeModel.LodLevels = _globeModelLODLevels;
            GlobeModel.PhysicsDownsampleLevel = GlobeModelPhysicsTargetDownsample;
            GlobeModel.Visible = true;

            TerrainLayerController layerController = GlobeModel.AddLayerController<GlobalTerrainLayerController>();
            layerController.TargetTextureSize = new Vector2Int(2048, 1024); // TODO Make these constants.
            layerController.EnableOverlay = true;
        }

        private void InitializeMaterial() {
            BaseMaterial = new Material(Shader.Find("Custom/Terrain/MultiDiffuseOverlay"));
            BaseMaterial.SetFloat("_DiffuseOpacity", 1);
            BaseMaterial.SetFloat("_Glossiness", ShaderSmoothness);
            BaseMaterial.SetFloat("_Metallic", ShaderMetallic);
            BaseMaterial.SetFloat("_OverlayOpacity", 0);

            // Set the base layer of the texture.
            TerrainModelTextureManager.Instance.GetTexture(
                new TerrainProductMetadata(GlobalMosaicUUID, UnrestrictedBoundingBox.Global, 0), 
                texture => BaseMaterial.SetTexture("_DiffuseBase", texture)
            );

            // Also add the base layer to the layer list.
            _globalLayers.Add(new TerrainLayer("Base", GlobalMosaicUUID));
        }

        #endregion

    }

}
