using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
///     Button script for testing various functionality.
/// </summary>
public class XRMenuTestButton : XRMenuElement {

    public override void OnTriggerDown(CustomControllerBehavior sender, Vector3 point, Vector3 normal, ClickedEventArgs e) {
        TerrainModelService terrainModelService = TerrainModelService.Instance;
        if (terrainModelService.DefaultPlanetModelIsVisible()) {
            string destFileName = $"test1.data";
            WebServiceManager.Instance?.DataElevationModelWebService.GetDEM(null, destFileName, () => {
                string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                TerrainModelBase terrainMesh = terrainModelService.Create(destFilePath);
                terrainModelService.ShowTerrainModel(terrainMesh);
            });
        }
        else {
            terrainModelService.ShowDefaultPlanetModel();
        }
    }

}
