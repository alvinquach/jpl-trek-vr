using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrekVRApplication {

    public class TempKeyboardInputController : MonoBehaviour {

        private IBookmarksWebService _bookmarksWebService = TrekBookmarksWebService.Instance;
        private IDigitalElevationModelWebService _dataElevationModelWebService = TrekDigitalElevationModelWebService.Instance;
        private IMosaicWebService _mosaicWebService = TrekMosaicWebService.Instance;

        private int count = 0;

        void Update() {

            if (Input.GetKeyUp(KeyCode.A)) {
                _bookmarksWebService.GetBookmarks(OnGetBookmarks);
            }

            if (Input.GetKeyUp(KeyCode.F)) {
                TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
                if (terrainModelManager.DefaultPlanetModelIsVisible()) {
                    BoundingBox bbox = new BoundingBox(-87.8906f, -21.4453f, -55.5469f, 1.4062f);
                    _dataElevationModelWebService.GetDEM(bbox, 1024, (demFilepath) => {
                        _mosaicWebService.GetMosaic(bbox, 1024, (textureFilepath) => {
                            TerrainModel terrainModel = terrainModelManager.Create(demFilepath, textureFilepath);
                            terrainModelManager.ShowTerrainModel(terrainModel);
                        });
                    });
                }
                else {
                    terrainModelManager.ShowDefaultPlanetModel();
                }
            }

        }

        private void OnGetBookmarks(IList<Bookmark> bookmarks) {
            Debug.Log("Hello " + ++count + "; count=" + bookmarks.Count);
        }

    }
    
}