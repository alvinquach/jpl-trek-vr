using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
///     Button script for testing various functionality.
/// </summary>
public class XRMenuTestSelectionButton : XRMenuElement {

    public override void OnTriggerDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        TerrainMeshController terrainMeshController = TerrainMeshController.Instance;
        if (terrainMeshController.DefaultPlanetMeshIsVisible()) {
            XRInteractablePlanet planet = terrainMeshController.GetComponentFromCurrentMesh<XRInteractablePlanet>();
            if (planet.InteractionMode == XRInteractablePlanetMode.Navigate) {
                planet.InteractionMode = XRInteractablePlanetMode.Select;
            } else {
                planet.InteractionMode = XRInteractablePlanetMode.Navigate;
            }
        }
        else {
            terrainMeshController.ShowDefaultPlanetMesh();
        }
    }

}
