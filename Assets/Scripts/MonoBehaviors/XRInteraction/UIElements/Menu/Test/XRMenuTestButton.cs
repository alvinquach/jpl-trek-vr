using System.IO;
using UnityEngine;

/// <summary>
///     Button script for testing various functionality.
/// </summary>
public class XRMenuTestButton : XRMenuElement {

    private IDataElevationModelWebService _dataElevationModelWebService = new MockDataElevationModelWebService();

    public override void OnTriggerDown(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) {
        TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
        if (terrainModelManager.DefaultPlanetModelIsVisible()) {
            string destFileName = $"test1.data";
            _dataElevationModelWebService.GetDEM(null, destFileName, () => {
                string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                TerrainModelBase terrainMesh = terrainModelManager.Create(destFilePath);
                terrainModelManager.ShowTerrainModel(terrainMesh);
            });
        }
        else {
            terrainModelManager.ShowDefaultPlanetModel();
        }
    }

}
