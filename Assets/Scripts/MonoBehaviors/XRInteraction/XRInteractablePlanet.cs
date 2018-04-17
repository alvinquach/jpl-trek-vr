using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class XRInteractablePlanet : XRInteractableObject {

    #region Grab Variables

    [SerializeField]
    private float _maxGrabDistance = 10.0f;

    [SerializeField]
    private float _angularDeceleration = 0.1f;

    [SerializeField]
    private float _linearDeceleration = 0.1f;

    [SerializeField]
    private int _decelerationSmoothing = 10;

    private bool _grabbed = false;
    private Vector3 _grabPoint;
    private float _grabRadius;
    private CustomControllerBehavior _grabber;

    #endregion

    #region Planet Navigation Variables

    private Vector3 _rotationAxis;
    private float _rotationAngle;
    private float _moveTime = 1.0f;
    private float _moveProgress = 0f;
    private bool _moving = false;

    #endregion

    public override void OnTriggerDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        if (Vector3.Distance(sender.transform.position, point) > _maxGrabDistance) {
            return;
        }
        _grabber = sender;
        _grabPoint = point;
        _grabRadius = Vector3.Distance(transform.position, point); // This should not change until another grab is made.
        _grabbed = true;
        Debug.Log("Grabbed something at " + point);

        _grabber.cursor.SetActive(true);
    }

    public override void OnTriggerUp(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        Ungrab();
    }

    public override void OnTriggerDoubleClick(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        Camera eye = sender.cameraRig.GetComponentInChildren<Camera>();
        GoTo(point - transform.position, eye.transform.position);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (_moving) {
            float deltaProgress = Time.deltaTime / _moveTime;
            transform.Rotate(_rotationAxis, _rotationAngle * deltaProgress, Space.World);
            _moveProgress += deltaProgress;
            if (_moveProgress >= 1f) {
                _moving = false;
            }
        }

        if (_grabbed && _grabber != null) {

            // Ray representing the forward direction of the controller.
            Ray forward = new Ray(_grabber.transform.position, _grabber.transform.forward);

            // Check if the controller is pointing outside of the planet's bounds.
            // If the controller is pointing outside of the planet, then ungrab the planet.
            float forwardDistFromObject = HandleUtility.DistancePointLine(transform.position, _grabber.transform.position, forward.GetPoint(_maxGrabDistance));
            if (forwardDistFromObject > _grabRadius) {
                Debug.Log("Lost grip on grabbed object (distance too large, " + forwardDistFromObject + ">" + _grabRadius + ")");
                Ungrab();
            }

            // If the controller is pointing within the planet's bounds, then rotate the planet.
            else {

                // Calculate the distance along the controller's forward direction where the new grab point is.
                float grabPointDistance = CalculateGrabPointDistance();

                // Use the distance to find the new grab point's coordinates.
                Vector3 newGrabPoint = forward.GetPoint(grabPointDistance);

                // Calcualte the rotation of the planet using the old and new grab points.
                Quaternion rotation = Quaternion.FromToRotation((_grabPoint - transform.position).normalized, (newGrabPoint - transform.position).normalized);

                // Rotate the planet.
                transform.rotation = rotation * transform.rotation;

                // Update the grab point.
                _grabPoint = newGrabPoint;

                _grabber.cursor.transform.position = _grabPoint;
            }
        }
    }

    public void GoTo(Vector2 coords, Vector3 cameraPosition) {

        // TODO Find a better way to do this.
        Quaternion asdf = transform.rotation;
        transform.Rotate(-coords.x, -coords.y, 0, Space.Self);
        Vector3 direction = transform.forward;

        transform.rotation = asdf;

        //_rotationAxis = Vector3.Cross(direction, cameraPosition - transform.position);
        //_rotationAngle = Vector3.Angle(direction, cameraPosition - transform.position);
        //transform.Rotate(_rotationAxis, _rotationAngle, Space.World);

        GoTo(direction, cameraPosition);
    }

    private void GoTo(Vector3 direction, Vector3 cameraPosition) {
        Debug.Log(direction);
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

    /// <summary>
    /// This function uses the law of cosines formula to find the distance between the controller and the current location of the grab point.
    /// </summary>
    private float CalculateGrabPointDistance() {

        // Vector between controller and the planet's transform.
        Vector3 controllerToObject = transform.position - _grabber.transform.position;
        float d = controllerToObject.magnitude;

        // Quadratic coefficients 'b' and 'c'. The 'a' coefficient is always 1.
        float b = -2 * d * Mathf.Cos(Vector3.Angle(controllerToObject, _grabber.transform.forward) * Mathf.Deg2Rad);
        float c = d * d - _grabRadius * _grabRadius;

        // Use quadratic formulat to find the distance.
        return (-b - Mathf.Sqrt(b * b - 4 * c)) / 2;

    }

    private void Ungrab() {
        if (_grabber != null) {
            _grabbed = false;
            _grabber.cursor.SetActive(false);
            _grabber = null;
        }
    }

}
