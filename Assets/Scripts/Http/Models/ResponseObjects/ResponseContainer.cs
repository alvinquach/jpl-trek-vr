using System.Collections.Generic;

public class ResponseContainer<T> : GenericResponseContainer where T : ResponseObject {

    public new T response;

}
