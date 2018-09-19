using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
///     Button script for testing various functionality.
/// </summary>
public class XRMenuTestButton : XRMenuElement {

    public override void OnTriggerDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        TerrainMeshController terrainMeshController = TerrainMeshController.Instance;
        if (terrainMeshController.DefaultPlanetMeshIsVisible()) {
            string destFileName = $"test1.data";
            WebServiceManager.Instance?.DataElevationModelWebService.GetDEM(null, destFileName, () => {
                string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                TerrainMesh terrainMesh = terrainMeshController.Create(destFilePath);
                terrainMeshController.ShowTerrainMesh(terrainMesh);
            });
        }
        else {
            terrainMeshController.ShowDefaultPlanetMesh();
        }
    }

}
