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
                XRInteractableGlobeTerrain globe = terrainModelController.GetComponentFromCurrentModel<XRInteractableGlobeTerrain>();
                if (globe.CurrentActivity == XRInteractableTerrainActivity.Default) {
                    globe.SwitchToActivity(XRInteractableTerrainActivity.BBoxSelection);
                }
                else {
                    globe.SwitchToActivity(XRInteractableTerrainActivity.Default);
                }
            }
            else {
                terrainModelController.ShowGlobeModel();
            }
        }

    }

}
