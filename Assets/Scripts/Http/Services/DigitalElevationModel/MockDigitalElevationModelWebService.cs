using System;
using System.IO;

namespace TrekVRApplication {

    /// <summary>
    ///     For testing purposes only.
    ///     Remove this implementation when it is no longer needed.
    /// </summary>
    public class MockDigitalElevationModelWebService : IDigitalElevationModelWebService {

        public void ClearCache() {
            // TODO Implement this
        }

        public void GetDEM(BoundingBox bbox, int size, Action<string> callback) {
            callback?.Invoke(Path.Combine(FilePath.PersistentRoot, FilePath.Test, "test1.data"));
        }

    }

}
