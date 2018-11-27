using System.Collections.Generic;

namespace TrekVRApplication {

    public class ResponseContainer<T> : GenericResponseContainer where T : ResponseObject {

        public new T response;

    }

}
