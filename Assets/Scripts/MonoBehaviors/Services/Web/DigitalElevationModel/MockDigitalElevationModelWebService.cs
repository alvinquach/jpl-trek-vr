using System;

/// <summary>
///     For testing purposes only.
///     Remove this implementation when it is no longer needed.
/// </summary>
public class MockDigitalElevationModelWebService : IDigitalElevationModelWebService {

    public void ClearCache() {
        // TODO Implement this
    }

    public void GetDEM(string resourceUrl, string destPath, Action callback) {
        //UnityWebRequest request = WebRequestUtils.Post("localhost:8080/rest/files/download", "D:/Alvin/Downloads/Trek DEMs/mola128_mola64_merge_90Nto90S_SimpleC_clon0_small.tif");
        //string dest = Path.Combine(FilePath.PersistentRoot, FilePath.Test, destPath);
        //FileRequest(request, dest, callback);
        callback?.Invoke();
    }

}

