
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
///     For testing purposes only.
///     Remove this implementation when it is no longer needed.
/// </summary>
public class MockDataElevationModelWebService : DataElevationModelWebService {

    public override void ClearCache() {
        // TODO Implement this
    }

    public override void GetDEM(string resourceUrl, string destPath, VoidCallback callback) {
        UnityWebRequest request = WebRequestUtils.Post("localhost:8080/rest/files/download", "D:/Alvin/Downloads/Trek DEMs/mola128_mola64_merge_90Nto90S_SimpleC_clon0_small.tif");
        string dest = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destPath);
        FileRequest(request, dest, callback);
    }

}

