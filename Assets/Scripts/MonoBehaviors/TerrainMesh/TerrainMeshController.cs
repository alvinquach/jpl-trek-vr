using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class TerrainMeshController : MonoBehaviour {

    private class TerrainMeshWrapper {

        /// <summary>
        ///     The wrapped TerrainMesh object.
        /// </summary>
        public TerrainMesh TerrainMesh;

        /// <summary>
        ///     The last time that this TerrainMesh's visibility was set from false to true using the SetVisible() method.
        /// </summary>
        public long LastVisible = 0L;

        public TerrainMeshWrapper(TerrainMesh terrainMesh) {
            TerrainMesh = terrainMesh;
        }

        /// <summary>
        ///     Sets the visibility of the TerrainMesh object by setting its GameObject to active or inactive.
        /// </summary>
        public void SetVisible(bool visible) {
            if (!visible) {
                TerrainMesh.gameObject.SetActive(false);
            }
            else if (!TerrainMesh.gameObject.activeSelf) {
                LastVisible = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                TerrainMesh.gameObject.SetActive(true);
            }
        }

    }

    /// <summary>
    ///     The maximum number of meshes that this controller can manage,
    ///     excluding the default planet mesh. Once the number of meshes
    ///     reaches this maximum, the oldest mesh will have to be deleted
    ///     when a new one is created.
    /// </summary>
    private const int MaxMeshes = 5;

    /// <summary>
    ///     The instance of the TerrainMeshController that is present in the scene.
    ///     There should only be one TerrainMeshController in the entire scene.
    /// </summary>
    public static TerrainMeshController Instance { get; private set; }

    [SerializeField]
    [Tooltip("The default material that is used as a base for new terrain meshes.")]
    private Material _defaultMaterial;

    /// <summary>
    ///     The default material that is used as a base for new terrain meshes.
    /// </summary>
    public Material DefaultMaterial {
        get { return _defaultMaterial; }
        set { _defaultMaterial = value; }
    }

    // TODO Generate this instead of drag and drop from Unity.
    // Also store it inside a wrapper?
    [SerializeField]
    private SphericalTerrainMesh _defaultPlanetMesh;

    // TODO Find out how the initial capacity for lists works in C#.
    private List<TerrainMeshWrapper> _terrainMeshes = new List<TerrainMeshWrapper>();

    void Awake() {

        if (Instance == null) {
            Instance = this;
        }

        if (_defaultPlanetMesh != null) {

            // TEMPORARY -- DO THIS PROPERLY
            _defaultPlanetMesh.DemFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.DataElevationModel,
                _defaultPlanetMesh.DemFilePath
            );

            // ALSO TEMPORARY
            _defaultPlanetMesh.AlbedoFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                FilePath.JetPropulsionLaboratory,
                FilePath.Texture,
                _defaultPlanetMesh.AlbedoFilePath
            );

            _defaultPlanetMesh.gameObject.SetActive(true);
        }
    }

    // TEMPORARY
    private int _counter = 0;

    public TerrainMesh Create(string demPath, string albedoPath = null) {
        GameObject newMesh = new GameObject();
        newMesh.transform.SetParent(transform);
        newMesh.name = $"Mesh {++_counter}";
        newMesh.SetActive(false);

        PlanarTerrainMesh terrainMesh = newMesh.AddComponent<PlanarTerrainMesh>();
        try {
            terrainMesh.Size = 1.5f;
            terrainMesh.HeightScale = 0.000002f;
            terrainMesh.LodLevels = 0;
            terrainMesh.BaseDownSampleLevel = 8;
            terrainMesh.DemFilePath = demPath;
            terrainMesh.AlbedoFilePath = albedoPath;
            terrainMesh.GenerateMeshData();
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
            Destroy(newMesh);
            return null;
        }
        
        // If the number of terrain meshes being managed already exceeds the max limit,
        // then we have to remove one to make space for the new one. We will remove the
        // one with the oldest LastActive time.
        if (_terrainMeshes.Count >= MaxMeshes) {
            
            // TODO Test this
            TerrainMeshWrapper wrapper = _terrainMeshes.OrderByDescending(w => w.LastVisible).Last();

            Destroy(wrapper.TerrainMesh.gameObject);
            _terrainMeshes.Remove(wrapper);
        }

        _terrainMeshes.Add(new TerrainMeshWrapper(terrainMesh));
        return terrainMesh;
    }

    public TerrainMesh CreatePartial(Vector4 boundingBox, string demPath, string albedoPath = null) {
        GameObject newMesh = new GameObject();
        newMesh.transform.SetParent(transform);
        newMesh.name = $"Mesh {++_counter}";
        newMesh.SetActive(false);

        PartialTerrainMesh terrainMesh = newMesh.AddComponent<PartialTerrainMesh>();
        try {
            terrainMesh.Radius = 3.39f;
            terrainMesh.HeightScale = 0f;
            // Bounding box is in the format (lonStart, latStart, lonEnd, latEnd)
            terrainMesh.BoundingBox = boundingBox;
            terrainMesh.LodLevels = 0;
            terrainMesh.GenerateMeshData();

            newMesh.transform.localPosition = Vector3.up;
            newMesh.transform.localScale = 0.25f * Vector3.one;
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
            Destroy(newMesh);
            return null;
        }

        // If the number of terrain meshes being managed already exceeds the max limit,
        // then we have to remove one to make space for the new one. We will remove the
        // one with the oldest LastActive time.
        if (_terrainMeshes.Count >= MaxMeshes) {

            // TODO Test this
            TerrainMeshWrapper wrapper = _terrainMeshes.OrderByDescending(w => w.LastVisible).Last();

            Destroy(wrapper.TerrainMesh.gameObject);
            _terrainMeshes.Remove(wrapper);
        }

        _terrainMeshes.Add(new TerrainMeshWrapper(terrainMesh));
        return terrainMesh;
    }

    /// <summary>
    ///     Whether the default planet mesh is currently visible as per this controller.
    /// </summary>
    public bool DefaultPlanetMeshIsVisible() {
        return _defaultPlanetMesh.gameObject.activeSelf;
    }

    /// <summary>
    ///     Shows the default planet mesh and hides the other TerrainMesh objects that
    ///     are managed by this controller.
    /// </summary>
    public void ShowDefaultPlanetMesh() {
        _terrainMeshes.ForEach(t => t.SetVisible(false));
        _defaultPlanetMesh.gameObject.SetActive(true);
    }

    /// <summary>
    ///     Set the TerrainMesh object to be visible by setting its GameObject to active.
    ///     Does not have any effect if the TerrainMesh's GameObject was already active,
    ///     or if the TerrainMesh is not managed by this controller.
    /// </summary>
    /// <returns>
    ///     True if the visiblity of the TerrainMesh objects in this
    ///     controller was changed. Otherwise, returns false.
    /// </returns>
    // TODO Write a separate method to clone the rotation.
    public bool ShowTerrainMesh(TerrainMesh terrainMesh, bool cloneRotation = false) {
        if (terrainMesh == null || terrainMesh.gameObject.activeSelf) {
            return false; // Retun false if the terrain mesh is already visible.
        }
        TerrainMeshWrapper wrapper = _terrainMeshes.Find(w => w.TerrainMesh == terrainMesh);
        if (wrapper != null) {
            HideAll();
            wrapper.SetVisible(true);
            if (cloneRotation) {
                terrainMesh.transform.rotation = _defaultPlanetMesh.transform.rotation;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    ///     Hides all the TerrainMesh objects managed by this controller by setting their
    ///     GameObjects to inactive. Includes the default planet mesh.
    /// </summary>
    public void HideAll() {
        _defaultPlanetMesh.gameObject.SetActive(false);
        _terrainMeshes.ForEach(w => w.SetVisible(false));
    }

    public T GetComponentFromCurrentMesh<T>() {

        // FIXME Change this so that it actually gets the component
        // fro the current mesh instead of the default mesh.
        return _defaultPlanetMesh.GetComponent<T>();
    }
    
}
