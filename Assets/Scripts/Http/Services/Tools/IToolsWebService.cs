using System;

namespace TrekVRApplication {

    public interface IToolsWebService {

        void GetDistance(string points, Action<string> callback);

    }

}
