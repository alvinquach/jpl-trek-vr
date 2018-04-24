using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookTowardsCamera : MonoBehaviour {

    [SerializeField]
    private Camera _camera;
	
	// Update is called once per frame
	void Update () {
		if (_camera) {
            transform.forward =  transform.position - _camera.transform.position;
        }
	}

}
