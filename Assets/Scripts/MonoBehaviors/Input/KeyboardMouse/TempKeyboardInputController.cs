using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TempKeyboardInputController : MonoBehaviour {

    private IBookmarksWebService _bookmarksWebService = JplBookmarksWebService.Instance;
    private IDataElevationModelWebService _dataElevationModelWebService = new MockDataElevationModelWebService();
    //private IDataElevationModelWebService _dataElevationModelWebService = JplDataElevationModelWebService.Instance;

    private int count = 0;

    void Update() {

        if (Input.GetKeyUp(KeyCode.A)) {
            _bookmarksWebService.GetBookmarks(OnGetBookmarks);
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            if (terrainModelManager.DefaultPlanetModelIsVisible()) {
                string destFileName = $"test1.data";
                _dataElevationModelWebService.GetDEM(null, destFileName, () => {
                    string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                    TerrainModelBase terrainMesh = terrainModelManager.Create(destFilePath);
                    terrainModelManager.ShowTerrainModel(terrainMesh);
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