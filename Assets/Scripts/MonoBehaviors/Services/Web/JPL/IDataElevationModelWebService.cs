using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDataElevationModelWebService : WebService {

    public abstract void GetDEM(string resourceUrl, FileCallback callback);

}
