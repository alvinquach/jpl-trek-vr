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
            if (terrainModelController.GlobeModelIsVisible()) {
                XRInteractableGlobe globe = terrainModelController.GetComponentFromCurrentModel<XRInteractableGlobe>();
                if (globe.InteractionMode == XRInteractableGlobeMode.Navigate) {
                    globe.SwitchToMode(XRInteractableGlobeMode.Select);
                }
                else {
                    globe.SwitchToMode(XRInteractableGlobeMode.Navigate);
                }
            }
            else {
                terrainModelController.ShowGlobeModel();
            }
        }

    }

}
