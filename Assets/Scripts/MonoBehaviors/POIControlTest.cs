using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIControlTest : MonoBehaviour {

    private Vector3 _startCoords;

    private Vector3 _targetCoords;

    private float _moveTime = 1.0f;

    private float _moveProgress = 0f;

    private bool _moving = false;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update() {

        if (_moving) {

            _moveProgress += Time.deltaTime / _moveTime;

            Vector3 _currentCoords = Vector3.Lerp(_startCoords, _targetCoords, _moveProgress);
            //PrintCoords("Current (" + _moveProgress + ")", _currentCoords);

            transform.forward = _currentCoords;

            if (_moveProgress >= 1f) {
                _moving = false;
            }
        }

        

	}

    public void GoTo(Vector2 coords, Vector3 cameraPosition) {
        _startCoords = transform.forward;
        PrintCoords("Start", _startCoords);

        _targetCoords = cameraPosition - transform.position;
        PrintCoords("Dest", _targetCoords);

        _moveProgress = 0;
        _moving = true;
    }

    public void GoTo(Vector3 direction, Vector3 cameraPosition) {
        _startCoords = transform.forward;

        _targetCoords = _startCoords + (cameraPosition - transform.position).normalized - direction.normalized;

        _moveProgress = 0;
        _moving = true;
    }


    private Vector2 directionToLatLong(Vector3 direction) {
        return new Vector2(
            Mathf.Atan2(direction.y, Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z)),
            Mathf.Atan2(direction.z, direction.x)
        );
    }

    private Vector3 LatLongToDirection(Vector2 latLong) {
        //float lat = latLong.x * Mathf.Deg2Rad;
        //float lon = latLong.y * Mathf.Deg2Rad;
        float lat = latLong.x;
        float lon = latLong.y;
        return new Vector3(
            Mathf.Sin(lat) * Mathf.Cos(lon),
            Mathf.Cos(lat),
            Mathf.Sin(lat) * Mathf.Sin(lon)
        );
    }

    private void PrintCoords(string label, Vector2 coord) {
        Debug.Log(label + ": (" + coord.x * Mathf.Rad2Deg + ", " + coord.y * Mathf.Rad2Deg + ")");
    }

}
