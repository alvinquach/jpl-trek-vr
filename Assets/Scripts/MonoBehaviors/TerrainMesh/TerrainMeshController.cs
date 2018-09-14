using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class TerrainMeshController : MonoBehaviour {

    /// <summary>
    ///     The maximum number of meshes that this controller can manage,
    ///     excluding the default planet mesh. Once the number of meshes
    ///     reaches this maximum, the oldest mesh will have to be deleted
    ///     when a new one is created.
    /// </summary>
    private const int MaxMeshes = 5;

    [SerializeField]
    private SphericalTerrainMesh _defaultPlanetMesh;

    // TODO Find out how the initial capacity for lists works in C#.
    private IList<TerrainMesh> _terrainMeshes = new List<TerrainMesh>();

    void Awake() {
        if (_defaultPlanetMesh != null) {

            // TEMPORARY -- DO THIS PROPERLY
            _defaultPlanetMesh.DEMFilePath = Path.Combine(
                FilePath.StreamingAssetsRoot,
                //FilePath.JetPropulsionLaboratory,
                FilePath.Texture,
                FilePath.DataElevationModel,
                _defaultPlanetMesh.DEMFilePath
            );

            _defaultPlanetMesh.gameObject.SetActive(true);
        }
        Create(null); // For testing -- delete after.
    }

    public void Create(string demPath) {
        GameObject newMesh = new GameObject();
        newMesh.transform.SetParent(transform);
        newMesh.SetActive(false);

        PlanarTerrainMesh terrainMesh = newMesh.AddComponent<PlanarTerrainMesh>();
        try {
            terrainMesh.InitMesh();
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
            Destroy(newMesh);
        }
    }

    public TerrainMesh[] GetAllTerrainMesh() {
        return transform.GetComponentsInChildren<TerrainMesh>();
    }

}
