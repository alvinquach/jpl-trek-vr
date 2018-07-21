using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataElevationModelWebService : WebService {

    public abstract void GetDEM(string resourceUrl, VoidCallback callback);

}
