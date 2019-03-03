using UnityEngine;
using System;

namespace TrekVRApplication {

    /// <summary>
    ///     Button script for testing various functionality.
    /// </summary>
    [Obsolete("Menu interaction is now done using an embedded browser. Use XRBrowser instead.")]
    public class XRMenuTestSelectionButton : XRMenuElement {

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            TerrainModelManager terrainModelController = TerrainModelManager.Instance;
            if (terrainModelController.GlobalPlanetModelIsVisible()) {
                XRInteractablePlanet planet = terrainModelController.GetComponentFromCurrentModel<XRInteractablePlanet>();
                if (planet.InteractionMode == XRInteractablePlanetMode.Navigate) {
                    planet.SwitchToMode(XRInteractablePlanetMode.Select);
                }
                else {
                    planet.SwitchToMode(XRInteractablePlanetMode.Navigate);
                }
            }
            else {
                terrainModelController.ShowGlobalPlanetModel();
            }
        }

    }

}
