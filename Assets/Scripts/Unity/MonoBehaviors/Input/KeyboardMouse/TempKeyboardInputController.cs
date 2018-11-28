using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrekVRApplication {

    public class TempKeyboardInputController : MonoBehaviour {

        private IBookmarksWebService _bookmarksWebService = TrekBookmarksWebService.Instance;
        //private IDigitalElevationModelWebService _dataElevationModelWebService = new MockDigitalElevationModelWebService();
        private IDigitalElevationModelWebService _dataElevationModelWebService = TrekDigitalElevationModelWebService.Instance;

        private int count = 0;

        void Update() {

            if (Input.GetKeyUp(KeyCode.A)) {
                _bookmarksWebService.GetBookmarks(OnGetBookmarks);
            }

            if (Input.GetKeyUp(KeyCode.F)) {
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

        private void OnGetBookmarks(IList<Bookmark> bookmarks) {
            Debug.Log("Hello " + ++count + "; count=" + bookmarks.Count);
        }

    }
    
}