using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEditor;

public class CustomControllerBehavior : MonoBehaviour {

    [SerializeField]
    private float _maxGrabDistance = 10.0f;

    [SerializeField]
    private GameObject _test;

    [SerializeField]
    private GameObject _cameraRig;

    [SerializeField]
    private float _speedMultiplier = 0.1f;

    private SteamVR_TrackedController _controller;

    private MovableObject _targetObject;

    private bool _grabbed = false;
    private Vector3 _grabPoint;
    private float _grabRadius;

    private bool _padClicked = false;

    private void OnEnable() {
        _controller = GetComponent<SteamVR_TrackedController>();
        _controller.TriggerClicked += TriggerClickedHandler;
        _controller.TriggerUnclicked += TriggerUnclickedHandler;
        _controller.PadClicked += PadClickedHandler;
        _controller.PadUnclicked += PadUnclickedHandler;
    }

    private void OnDisable() {
        _controller.TriggerClicked -= TriggerClickedHandler;
        _controller.TriggerUnclicked -= TriggerUnclickedHandler;
        _controller.PadClicked -= PadClickedHandler;
        _controller.PadUnclicked -= PadUnclickedHandler;
    }

    #region Controller Event Handlers

    private void TriggerClickedHandler(object sender, ClickedEventArgs e) {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _maxGrabDistance)) {
            //_targetObject = hit.transform.GetComponent<MovableObject>();
            //if (_targetObject != null) {
            //    _grabPoint = hit.point;
            //    _grabRadius = Vector3.Distance(_targetObject.transform.position, _grabPoint); // This should not change until another grab is made.
            //    _targetObject.grabbed = _grabbed = true;
            //    Debug.Log("Grabbed something at " + _grabPoint);

            //    _test.SetActive(true);
            //}

            POIControlTest asdf = hit.transform.GetComponent<POIControlTest>();
            Camera eye = _cameraRig.GetComponentInChildren<Camera>();
            //Debug.Log(eye.transform.position);
            if (asdf != null) {
                asdf.GoTo(hit.point - hit.transform.position, eye.transform.position);
            }
        }
    }

    private void TriggerUnclickedHandler(object sender, ClickedEventArgs e) {
        Ungrab();
    }

    private void PadClickedHandler(object sender, ClickedEventArgs e) {
        Debug.Log("Pad clicked at (" + e.padX + ", " + e.padY + ")");
        _padClicked = true;
    }

    private void PadUnclickedHandler(object sender, ClickedEventArgs e) {
        Debug.Log("Pad unclicked at (" + e.padX + ", " + e.padY + ")");
        _padClicked = false;
    }

    #endregion


    private void Update() {

        // Update the target object if it is grabbed.
        if (_grabbed && _targetObject != null) {

            Vector3 targetOjectPosition = _targetObject.transform.position;

            // Ray representing the forward direction of the controller.
            Ray forward = new Ray(transform.position, transform.forward);

            // Check if the controller is pointing outside of the object's bounds.
            // If the controller is pointing outside of the object, then ungrab the object.
            float forwardDistFromObject = HandleUtility.DistancePointLine(targetOjectPosition, transform.position, forward.GetPoint(_maxGrabDistance));
            if (forwardDistFromObject > _grabRadius) {
                Debug.Log("Lost grip on grabbed object (distance too large)");
                Ungrab();
            }

            // If the controller is pointing within the object, then rotate the object.
            else {

                // Calculate the distance along the controller's forward direction where the new grab point is.
                float grabPointDistance = CalculateGrabPointDistance();

                // Use the distance to find the new grab point's coordinates.
                Vector3 newGrabPoint = forward.GetPoint(grabPointDistance);

                // Calcualte the rotation of the target object using the old and new grab points.
                Quaternion rotation = Quaternion.FromToRotation((_grabPoint - targetOjectPosition).normalized, (newGrabPoint - targetOjectPosition).normalized);

                // Rotate the target object.
                _targetObject.transform.rotation = rotation * _targetObject.transform.rotation;

                // Update the grab point.
                _grabPoint = newGrabPoint;

                _test.transform.position = _grabPoint;
            }
        }

        // Update player movement
        if (_padClicked) {

            // Get the pad position from the controller device.
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)_controller.controllerIndex);
            Vector2 axis = device.GetAxis();

            // Move the player based on controller direction and pad position.
            // Movement is limited along the xz-plane.
            Vector3 direction = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
            _cameraRig.transform.position += (axis.y > 0 ? 1 : -1) * _speedMultiplier * direction;

        }

    }

    #region Helper Functions

    private void Ungrab() {
        if (_targetObject != null) {
            _targetObject.grabbed = _grabbed = false;
            _targetObject = null;

            _test.SetActive(false);
        }
    }

    /// <summary>
    /// This function uses the law of cosines formula to find the distance between the controller and the current location of the grab point.
    /// </summary>
    private float CalculateGrabPointDistance() {

        // Vector between controller and the target object's transform.
        Vector3 controllerToObject = _targetObject.transform.position - transform.position;
        float d = controllerToObject.magnitude;

        // Quadratic coefficients 'b' and 'c'. The 'a' coefficient is always 1.
        float b = -2 * d * Mathf.Cos(Vector3.Angle(controllerToObject, transform.forward) * Mathf.Deg2Rad);
        float c = d * d - _grabRadius * _grabRadius;

        // Use quadratic formulat to find the distance.
        return (-b - Mathf.Sqrt(b * b - 4 * c)) / 2;

    }

    #endregion

}