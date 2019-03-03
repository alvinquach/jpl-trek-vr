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
        [Tooltip("The material that is used as a base for new terrain models' enabled material.")]
        private Material _baseEnabledMaterial;

        /// <summary>
        ///     The material that is used as a base for new terrain models' enabled material.
        /// </summary>
        public Material BaseEnabledMaterial {
            get { return _baseEnabledMaterial; }
        }

        [SerializeField]
        [Tooltip("The material that is used as a base for new terrain models' disabled material.")]
        private Material _baseDisabledMaterial;

        /// <summary>
        ///     The material that is used as a base for new terrain models' disabled material.
        /// </summary>
        public Material BaseDisabledMaterial {
            get { return _baseDisabledMaterial; }
        }

        #region Global planet fields/properties.

        [SerializeField]
        private string _globalPlanetDEMFilepath;

        [SerializeField]
        private string _globalPlanetAlbedoFilepath;

        [SerializeField]
        private int _globalPlanetBaseDownsampleLevel = 1;

        [SerializeField]
        private int _globalPlanetLODLevels = 2;

        public GlobalTerrainModel GlobalPlanetModel { get; private set; }

        public Texture GlobalPlanetTexture { get; set; }

        #endregion

        // TODO Find out how the initial capacity for lists works in C#.
        private List<TerrainModel> _terrainModels = new List<TerrainModel>();

        private GameObject _terrainModelsContainer;

        public TerrainModel CurrentVisibleModel {
            get {
                if (GlobalPlanetModel.Visible) {
                    return GlobalPlanetModel;
                }
                return _terrainModels.Find(model => model.Visible);
            }
        }

        void Awake() {

            if (Instance == null) {
                Instance = this;
            }

            // Create a game object that will contain all the terrain model game objects.
            _terrainModelsContainer = new GameObject();
            _terrainModelsContainer.name = GameObjectName.TerrainModelsContainer;

            // Create the global planet game object.
            GameObject globalPlanetModelGameObject = new GameObject();
            globalPlanetModelGameObject.transform.parent = _terrainModelsContainer.transform;
            globalPlanetModelGameObject.name = typeof(Mars).Name;
            GlobalPlanetModel = globalPlanetModelGameObject.AddComponent<GlobalTerrainModel>();

            // TEMPORARY -- DO THIS PROPERLY
            GlobalPlanetModel.DemFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.DigitalElevationModel,
                _globalPlanetDEMFilepath
            );

            // ALSO TEMPORARY
            GlobalPlanetModel.AlbedoFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.Texture,
                _globalPlanetAlbedoFilepath
            );

            GlobalPlanetModel.Radius = Mars.Radius;
            GlobalPlanetModel.BaseDownSampleLevel = _globalPlanetBaseDownsampleLevel;
            GlobalPlanetModel.LodLevels = _globalPlanetLODLevels;
            GlobalPlanetModel.Visible = true;

            // Shift up 1 meter.
            _terrainModelsContainer.transform.position = Vector3.up;

        }

        // TEMPORARY
        private int _counter = 0;

        [Obsolete("Planar terrains should no longer be created. Use CreatePartial() to create partial terrain instead.")]
        public TerrainModel Create(string demPath, string albedoPath = null) {
            GameObject terrainModelContainer = new GameObject();
            terrainModelContainer.transform.SetParent(_terrainModelsContainer.transform);
            terrainModelContainer.name = $"Model {++_counter}";
            terrainModelContainer.SetActive(false);

            PlanarTerrainModel terrainModel = terrainModelContainer.AddComponent<PlanarTerrainModel>();
            try {
                terrainModel.Size = 1.5f;
                terrainModel.HeightScale = 0.000002f;
                terrainModel.LodLevels = 0;
                terrainModel.BaseDownSampleLevel = 4;
                terrainModel.DemFilePath = demPath;
                terrainModel.AlbedoFilePath = albedoPath;
                terrainModel.InitModel();
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
                Destroy(terrainModelContainer);
                return null;
            }

            return AddTerrainModel(terrainModel);
        }

        public TerrainModel CreatePartial(BoundingBox boundingBox, string demPath, string albedoPath = null) {
            GameObject terrainModelContainer = new GameObject();
            terrainModelContainer.transform.SetParent(_terrainModelsContainer.transform);
            terrainModelContainer.name = $"Model {++_counter}";
            terrainModelContainer.SetActive(false);

            PartialTerrainModel terrainModel = terrainModelContainer.AddComponent<PartialTerrainModel>();
            try {
                terrainModel.Radius = 3.39f;
                terrainModel.HeightScale = 1e-6f;
                // Bounding box is in the format (lonStart, latStart, lonEnd, latEnd)
                terrainModel.BoundingBox = boundingBox;
                terrainModel.LodLevels = 0;
                terrainModel.InitModel();

                terrainModelContainer.transform.localPosition = Vector3.up;
                terrainModelContainer.transform.localScale = 0.25f * Vector3.one;
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
                Destroy(terrainModelContainer);
                return null;
            }

            return AddTerrainModel(terrainModel);
        }

        /// <summary>
        ///     Whether the global planet model is currently visible as per this controller.
        /// </summary>
        public bool GlobalPlanetModelIsVisible() {
            return GlobalPlanetModel.Visible;
        }

        /// <summary>
        ///     Shows the global planet model and hides the other TerrainModelBase objects
        ///     that are managed by this controller.
        /// </summary>
        public void ShowGlobalPlanetModel() {
            _terrainModels.ForEach(t => t.Visible = false);
            GlobalPlanetModel.Visible = true;
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
                terrainModel.transform.rotation = GlobalPlanetModel.transform.rotation;
            }
            return true;
        }

        /// <summary>
        ///     Hides all the TerrainModelBase objects managed by this controller by setting their
        ///     GameObjects to inactive. Includes the default planet model.
        /// </summary>
        public void HideAll() {
            GlobalPlanetModel.Visible = false;
            _terrainModels.ForEach(w => w.Visible = false);
        }

        public T GetComponentFromCurrentModel<T>() {

            // FIXME Change this so that it actually gets the component
            // for the current model instead of the global model.
            return GlobalPlanetModel.GetComponent<T>();
        }

        public Transform GetGlobalPlanetModelTransform() {
            return GlobalPlanetModel.transform;
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
