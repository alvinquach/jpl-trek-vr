using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIControlTest : MonoBehaviour {

    private Vector3 _rotationAxis;
    private float _rotationAngle;

    private float _moveTime = 1.0f;

    private float _moveProgress = 0f;

    private bool _moving = false;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update() {
        if (_moving) {
            float deltaProgress = Time.deltaTime / _moveTime;
            transform.Rotate(_rotationAxis, _rotationAngle * deltaProgress, Space.World);
            _moveProgress += deltaProgress;
            if (_moveProgress >= 1f) {
                _moving = false;
            }
        }
	}

    public void GoTo(Vector2 coords, Vector3 cameraPosition) {
        GoTo(LatLongToDirection(coords), cameraPosition);
    }

    public void GoTo(Vector3 direction, Vector3 cameraPosition) {
        _rotationAxis = Vector3.Cross(direction, cameraPosition - transform.position);
        _rotationAngle = Vector3.Angle(direction, cameraPosition - transform.position);
        _moveProgress = 0;
        _moving = true;
    }

    private Vector2 DirectionToLatLong(Vector3 direction) {
        return new Vector2(
            Mathf.Atan2(direction.y, Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z)) * Mathf.Rad2Deg,
            Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg
        );
    }

    private Vector3 LatLongToDirection(Vector2 latLong) {
        float lat = latLong.x * Mathf.Deg2Rad;
        float lon = latLong.y * Mathf.Deg2Rad;
        //float lat = latLong.x;
        //float lon = latLong.y;
        return new Vector3(
            Mathf.Sin(lat) * Mathf.Cos(lon),
            Mathf.Cos(lat),
            Mathf.Sin(lat) * Mathf.Sin(lon)
        );
    }

    private void PrintCoords(string label, Vector3 vector) {
        Debug.Log(label + ": (" + vector.x + ", " + vector.y + ", " + vector.z + ")");
    }

}
