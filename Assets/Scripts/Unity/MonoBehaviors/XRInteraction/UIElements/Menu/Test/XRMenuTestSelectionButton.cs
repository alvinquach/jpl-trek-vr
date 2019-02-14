using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Button script for testing various functionality.
    /// </summary>
    public class XRMenuTestSelectionButton : XRMenuElement {

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            TerrainModelManager terrainModelController = TerrainModelManager.Instance;
            if (terrainModelController.DefaultPlanetModelIsVisible()) {
                XRInteractablePlanet planet = terrainModelController.GetComponentFromCurrentModel<XRInteractablePlanet>();
                if (planet.InteractionMode == XRInteractablePlanetMode.Navigate) {
                    planet.InteractionMode = XRInteractablePlanetMode.Select;
                }
                else {
                    planet.InteractionMode = XRInteractablePlanetMode.Navigate;
                }
            }
            else {
                terrainModelController.ShowDefaultPlanetModel();
            }
        }

    }

}
