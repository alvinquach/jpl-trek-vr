using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class TerrainModelManager : MonoBehaviour {

        /// <summary>
        ///     The maximum number of models that this controller can manage,
        ///     excluding the default planet model. Once the number of models
        ///     reaches this maximum, the oldest model will have to be deleted
        ///     when a new one is created.
        /// </summary>
        private const int MaxModels = 5;

        /// <summary>
        ///     The instance of the TerrainModelManager that is present in the scene.
        ///     There should only be one TerrainModelManager in the entire scene.
        /// </summary>
        public static TerrainModelManager Instance { get; private set; }

        [SerializeField]
        [Tooltip("The material that is used as a base for new terrain models.")]
        private Material _baseMaterial;

        /// <summary>
        ///     The material that is used as a base for new terrain models.
        /// </summary>
        public Material BaseMaterial {
            get { return _baseMaterial; }
        }

        #region Globe model fields/properties.

        [SerializeField]
        private string _globalDEMFilepath;

        [SerializeField]
        private int _globeModelBaseDownsampleLevel = 1;

        [SerializeField]
        private int _globeModelLODLevels = 2;

        public GlobeTerrainModel GlobeModel { get; private set; }

        #endregion

        // TODO Find out how the initial capacity for lists works in C#.
        private List<TerrainModel> _terrainModels = new List<TerrainModel>();

        private GameObject _terrainModelsContainer;

        public TerrainModel CurrentVisibleModel {
            get {
                if (GlobeModel.Visible) {
                    return GlobeModel;
                }
                return _terrainModels.Find(model => model.Visible);
            }
        }

        /// <summary>
        ///     Whether the terrain models are set to render in opaque (enabled) mode
        ///     or semi-transparent (disabled) mode.
        /// </summary>
        public bool TerrainRenderMode { get; private set; } = true;

        public event Action<bool> OnRenderModeChange = opaque => { };

        private void Awake() {

            if (Instance == null) {
                Instance = this;
            }

            // Create a game object that will contain all the terrain model game objects.
            _terrainModelsContainer = new GameObject(GameObjectName.TerrainModelsContainer) {
                layer = (int)CullingLayer.Terrain
            };

            // Create the globe model conatiner game object.
            GameObject globeModelGameObject = new GameObject(typeof(Mars).Name) {
                layer = (int)CullingLayer.Terrain
            };
            globeModelGameObject.transform.parent = _terrainModelsContainer.transform;
            GlobeModel = globeModelGameObject.AddComponent<GlobeTerrainModel>();

            // TEMPORARY -- DO THIS PROPERLY
            GlobeModel.DemFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.DigitalElevationModel,
                _globalDEMFilepath
            );

            GlobeModel.Radius = Mars.Radius;
            GlobeModel.BaseDownSampleLevel = _globeModelBaseDownsampleLevel;
            GlobeModel.LodLevels = _globeModelLODLevels;
            GlobeModel.Visible = true;

            // Shift up 1 meter.
            _terrainModelsContainer.transform.position = Vector3.up;

        }

        // TEMPORARY
        private int _counter = 0;

        public TerrainModel CreateSectionModel(BoundingBox boundingBox, string demPath, string albedoPath = null) {
            GameObject terrainModelContainer = new GameObject($"Model {++_counter}") {
                layer = (int)CullingLayer.Terrain
            };
            terrainModelContainer.transform.SetParent(_terrainModelsContainer.transform, false);
            terrainModelContainer.SetActive(false);

            SectionTerrainModel terrainModel = terrainModelContainer.AddComponent<SectionTerrainModel>();
            try {
                terrainModel.Radius = Mars.Radius;
                terrainModel.BoundingBox = boundingBox;
                terrainModel.LodLevels = 0;
                terrainModel.InitModel();
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
                Destroy(terrainModelContainer);
                return null;
            }

            return AddTerrainModel(terrainModel);
        }

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
            _terrainModels.ForEach(t => t.Visible = false);
            GlobeModel.Visible = true;
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
            GlobeModel.Visible = false;
            _terrainModels.ForEach(w => w.Visible = false);
        }

        public T GetComponentFromCurrentModel<T>() {

            // FIXME Change this so that it actually gets the component
            // for the current model instead of the globe model.
            return GlobeModel.GetComponent<T>();
        }

        public Transform GetGlobeModelTransform() {
            return GlobeModel.transform;
        }

        public void SetTerrainRenderMode(bool mode) {
            if (TerrainRenderMode != mode) {
                TerrainRenderMode = mode;
                OnRenderModeChange.Invoke(mode);
            }
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

    }

}
