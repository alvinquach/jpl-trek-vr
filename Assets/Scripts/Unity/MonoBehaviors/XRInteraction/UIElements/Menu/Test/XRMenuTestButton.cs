using System;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Button script for testing various functionality.
    /// </summary>
    [Obsolete("Menu interaction is now done using an embedded browser. Use XRBrowser instead.")]
    public class XRMenuTestButton : XRMenuElement {

        private IDigitalElevationModelWebService _dataElevationModelWebService = new MockDigitalElevationModelWebService();

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            if (terrainModelManager.DefaultPlanetModelIsVisible()) {
                _dataElevationModelWebService.GetDEM(new BoundingBox(-87.8906f, -21.4453f, -55.5469f, 1.4062f), 1024, (filepath) => {
                    TerrainModel terrainModel = terrainModelManager.Create(filepath);
                    terrainModelManager.ShowTerrainModel(terrainModel);
                });
            }
            else {
                terrainModelManager.ShowDefaultPlanetModel();
            }
        }

    }

}
