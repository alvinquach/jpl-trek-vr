using System.IO;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Button script for testing various functionality.
    /// </summary>
    public class XRMenuTestButton : XRMenuElement {

        private IDigitalElevationModelWebService _dataElevationModelWebService = new MockDigitalElevationModelWebService();

        public override void OnTriggerDown(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            if (terrainModelManager.DefaultPlanetModelIsVisible()) {
                string destFileName = $"test1.data";
                _dataElevationModelWebService.GetDEM(new BoundingBox(-87.8906f, -21.4453f, -55.5469f, 1.4062f), 1024, () => {
                    string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                    TerrainModel terrainModel = terrainModelManager.Create(destFilePath);
                    terrainModelManager.ShowTerrainModel(terrainModel);
                });
            }
            else {
                terrainModelManager.ShowDefaultPlanetModel();
            }
        }

    }

}
