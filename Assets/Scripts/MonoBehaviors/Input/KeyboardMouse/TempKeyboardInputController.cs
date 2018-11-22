using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TempKeyboardInputController : MonoBehaviour {

    private IBookmarksWebService _bookmarksWebService = JplBookmarksWebService.Instance;
    private IDataElevationModelWebService _dataElevationModelWebService = JplDataElevationModelWebService.Instance;

    private int count = 0;

    void Update() {

        if (Input.GetKeyUp(KeyCode.A)) {
            _bookmarksWebService.GetBookmarks(OnGetBookmarks);
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            string destFileName = $"test{++count}.data";
           _dataElevationModelWebService.GetDEM(null, destFileName, () => {
                string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
                TerrainModelBase terrainModel = terrainModelManager.Create(destFilePath, "D:/Alvin/Downloads/Trek DEMs/exportImage.png");
                terrainModelManager.ShowTerrainModel(terrainModel);
            });
        }

    }

    private void OnGetBookmarks(IList<Bookmark> bookmarks) {
        Debug.Log("Hello " + ++count + "; count=" + bookmarks.Count);
    }

}