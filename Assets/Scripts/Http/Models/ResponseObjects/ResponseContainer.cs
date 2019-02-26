using System;
using System.Collections.Generic;

namespace TrekVRApplication {

    [Obsolete]
    public class ResponseContainer<T> : GenericResponseContainer where T : ResponseObject {

        public new T response;

    }

}
