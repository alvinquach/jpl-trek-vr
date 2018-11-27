using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TempKeyboardInputController : MonoBehaviour {

    private IBookmarksWebService _bookmarksWebService = JplBookmarksWebService.Instance;
    //private IDigitalElevationModelWebService _dataElevationModelWebService = new MockDigitalElevationModelWebService();
    private JplDigitalElevationModelWebService _dataElevationModelWebService = JplDigitalElevationModelWebService.Instance;

    private int count = 0;

    void Update() {

        if (Input.GetKeyUp(KeyCode.A)) {
            _bookmarksWebService.GetBookmarks(OnGetBookmarks);
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            if (terrainModelManager.DefaultPlanetModelIsVisible()) {
                string resourceUrl = "http://ec2-54-177-76-230.us-west-1.compute.amazonaws.com/arcgis/rest/services/mola128_mola64_merge_90Nto90S_SimpleC_clon0/ImageServer//exportImage?bbox=-87.8906%2C-21.4453%2C-55.5469%2C1.4062&size=1024%2C1024&imageSR=%7B%22wkt%22+%3A+%22PROJCS%5B%5C%22mars_stereo%5C%22%2CGEOGCS%5B%5C%22GCS_Mars_2000%5C%22%2CDATUM%5B%5C%22Mars_2000%5C%22%2CSPHEROID%5B%5C%22Mars_2000_IAU_IAG%5C%22%2C+3396190%2C0%5D%5D%2CPRIMEM%5B%5C%22Greenwich%5C%22%2C0.0%5D%2CUNIT%5B%5C%22Degree%5C%22%2C0.0174532925199433%5D%5D%2CPROJECTION%5B%5C%22Stereographic%5C%22%5D%2CPARAMETER%5B%5C%22false_easting%5C%22%2C0.0%5D%2CPARAMETER%5B%5C%22false_northing%5C%22%2C0.0%5D%2CPARAMETER%5B%5C%22central_meridian%5C%22%2C-54%5D%2CPARAMETER%5B%5C%22scale_factor%5C%22%2C1.0%5D%2CUNIT%5B%5C%22Meter%5C%22%2C1.0%5D%5D%22%7D&pixelType=UNKNOWN&noDataInterpretation=esriNoDataMatchAny&interpolation=RSP_BilinearInterpolation&format=tiff&f=image";
                string destFileName = $"test4.data";
                string destFilePath = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destFileName);
                _dataElevationModelWebService.GetDEM(resourceUrl, destFilePath, () => {
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