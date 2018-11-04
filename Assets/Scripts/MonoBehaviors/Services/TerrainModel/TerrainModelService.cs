using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class TerrainModelService : MonoBehaviour {

    /// <summary>
    ///     The maximum number of models that this controller can manage,
    ///     excluding the default planet model. Once the number of models
    ///     reaches this maximum, the oldest model will have to be deleted
    ///     when a new one is created.
    /// </summary>
    private const int MaxModels = 5;

    /// <summary>
    ///     The instance of the TerrainModelService that is present in the scene.
    ///     There should only be one TerrainModelService in the entire scene.
    /// </summary>
    public static TerrainModelService Instance { get; private set; }

    [SerializeField]
    [Tooltip("The default material that is used as a base for new terrain models.")]
    private Material _defaultMaterial;

    /// <summary>
    ///     The default material that is used as a base for new terrain models.
    /// </summary>
    public Material DefaultMaterial {
        get { return _defaultMaterial; }
        set { _defaultMaterial = value; }
    }

    // TODO Generate this instead of drag and drop from Unity.
    // Also store it inside a wrapper?
    [SerializeField]
    private GlobalTerrainModel _defaultPlanetModel;

    // TODO Find out how the initial capacity for lists works in C#.
    private List<TerrainModelBase> _terrainModels = new List<TerrainModelBase>();

    private GameObject _terrainModelsContainer;

    void Awake() {

        if (Instance == null) {
            Instance = this;
        }

        if (!_terrainModelsContainer) {
            GameObject terrainModelsContainer = GameObject.Find(GameObjectName.TerrainModelsContainer);
            _terrainModelsContainer = terrainModelsContainer ? terrainModelsContainer : gameObject;
        }

        if (_defaultPlanetModel) {

            // TEMPORARY -- DO THIS PROPERLY
            _defaultPlanetModel.DemFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.DataElevationModel,
                _defaultPlanetModel.DemFilePath
            );

            // ALSO TEMPORARY
            _defaultPlanetModel.AlbedoFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.Texture,
                _defaultPlanetModel.AlbedoFilePath
            );

            _defaultPlanetModel.Visible = true;
        }

    }

    // TEMPORARY
    private int _counter = 0;

    [Obsolete("Planar terrains should no longer be created. Use CreatePartial() to create partial terrain instead.")]
    public TerrainModelBase Create(string demPath, string albedoPath = null) {
        GameObject terrainModelContainer = new GameObject();
        terrainModelContainer.transform.SetParent(_terrainModelsContainer.transform);
        terrainModelContainer.name = $"Model {++_counter}";
        terrainModelContainer.SetActive(false);

        PlanarTerrainModel terrainModel = terrainModelContainer.AddComponent<PlanarTerrainModel>();
        try {
            terrainModel.Size = 1.5f;
            terrainModel.HeightScale = 0.000002f;
            terrainModel.LodLevels = 0;
            terrainModel.BaseDownSampleLevel = 8;
            terrainModel.DemFilePath = demPath;
            terrainModel.AlbedoFilePath = albedoPath;
            terrainModel.GenerateMeshData();
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
            Destroy(terrainModelContainer);
            return null;
        }

        return AddTerrainModel(terrainModel);
    }

    public TerrainModelBase CreatePartial(Vector4 boundingBox, string demPath, string albedoPath = null) {
        GameObject terrainModelContainer = new GameObject();
        terrainModelContainer.transform.SetParent(_terrainModelsContainer.transform);
        terrainModelContainer.name = $"Model {++_counter}";
        terrainModelContainer.SetActive(false);

        PartialTerrainModel terrainModel = terrainModelContainer.AddComponent<PartialTerrainModel>();
        try {
            terrainModel.Radius = 3.39f;
            terrainModel.HeightScale = 0f;
            // Bounding box is in the format (lonStart, latStart, lonEnd, latEnd)
            terrainModel.BoundingBox = boundingBox;
            terrainModel.LodLevels = 0;
            terrainModel.GenerateMeshData();

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
    ///     Whether the default planet model is currently visible as per this controller.
    /// </summary>
    public bool DefaultPlanetModelIsVisible() {
        return _defaultPlanetModel.Visible;
    }

    /// <summary>
    ///     Shows the default planet model and hides the other TerrainModelBase objects
    ///     that are managed by this controller.
    /// </summary>
    public void ShowDefaultPlanetModel() {
        _terrainModels.ForEach(t => t.Visible = false);
        _defaultPlanetModel.Visible = true;
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
    public bool ShowTerrainModel(TerrainModelBase terrainModel, bool cloneRotation = false) {
        if (terrainModel == null || terrainModel.Visible) {
            return false; // Retun false if the terrain model is already visible.
        }
        HideAll();
        terrainModel.Visible = true;
        if (cloneRotation) {
            terrainModel.transform.rotation = _defaultPlanetModel.transform.rotation;
        }
        return true;
    }

    /// <summary>
    ///     Hides all the TerrainModelBase objects managed by this controller by setting their
    ///     GameObjects to inactive. Includes the default planet model.
    /// </summary>
    public void HideAll() {
        _defaultPlanetModel.Visible = false;
        _terrainModels.ForEach(w => w.Visible = false);
    }

    public T GetComponentFromCurrentModel<T>() {

        // FIXME Change this so that it actually gets the component
        // for the current model instead of the default model.
        return _defaultPlanetModel.GetComponent<T>();
    }

    private TerrainModelBase AddTerrainModel(TerrainModelBase terrainModel) {

        // If the number of terrain models being managed already exceeds the max limit,
        // then we have to remove one to make space for the new one. We will remove the
        // one with the oldest LastActive time.
        if (_terrainModels.Count >= MaxModels) {

            // TODO Test this
            TerrainModelBase oldest = _terrainModels.OrderByDescending(w => w.LastVisible).Last();

            Destroy(oldest.gameObject);
            _terrainModels.Remove(oldest);
        }

        _terrainModels.Add(terrainModel);
        return terrainModel;
    }

}
