using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrekVRApplication {

    public class TempKeyboardInputController : MonoBehaviour {

        private IBookmarksWebService _bookmarksWebService = TrekBookmarksWebService.Instance;
        //private IDigitalElevationModelWebService _dataElevationModelWebService = new MockDigitalElevationModelWebService();
        private TrekDigitalElevationModelWebService _dataElevationModelWebService = TrekDigitalElevationModelWebService.Instance;

        private int count = 0;

        void Update() {

            if (Input.GetKeyUp(KeyCode.A)) {
                _bookmarksWebService.GetBookmarks(OnGetBookmarks);
            }

            if (Input.GetKeyUp(KeyCode.F)) {
                TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
                if (terrainModelManager.DefaultPlanetModelIsVisible()) {
                    string destFileName = $"test4.data";
                    string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                    _dataElevationModelWebService.GetDEM(new BoundingBox(-87.8906f, -21.4453f, -55.5469f, 1.4062f), 1024, () => {
                        TerrainModel terrainModel = terrainModelManager.Create(destFilePath);
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