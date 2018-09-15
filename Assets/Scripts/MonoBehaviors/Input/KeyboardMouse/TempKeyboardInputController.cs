using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TempKeyboardInputController : MonoBehaviour {

    private int count = 0;

    void Update() {

        if (Input.GetKeyUp(KeyCode.A)) {
            WebServiceManager.Instance?.BookmarksWebService.GetBookmarks(OnGetBookmarks);
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            string destFileName = $"test{++count}.data";
            WebServiceManager.Instance?.DataElevationModelWebService.GetDEM(null, destFileName, () => {
                string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                TerrainMeshController terrainMeshController = TerrainMeshController.Instance;
                TerrainMesh terrainMesh = terrainMeshController.Create(destFilePath);
                terrainMeshController.ShowTerrainMesh(terrainMesh);
            });
        }

    }

    private void OnGetBookmarks(IList<Bookmark> bookmarks) {
        Debug.Log("Hello " + ++count + "; count=" + bookmarks.Count);
    }

}