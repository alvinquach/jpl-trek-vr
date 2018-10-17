using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class XRInteractablePlanet : XRInteractableObject {

    // TODO Move these somewhere else?
    private const int CoordinateIndicatorSegmentCount = 72;
    private const float CoordinateIndicatorThickness = 0.0069f;
    private const float CoordinateIndicatorActiveThickness = 0.01337f;

    private XRInteractablePlanetMode _interactionMode = XRInteractablePlanetMode.Navigate;
    public XRInteractablePlanetMode InteractionMode {
        get {
            return _interactionMode;
        }
        set {
            if (value == XRInteractablePlanetMode.Select) {
                _lonSelectionStartIndicator.startWidth = CoordinateIndicatorActiveThickness;
                _lonSelectionStartIndicator.enabled = true;
            }
            else {
                CancelSelection(true);
            }
            _interactionMode = value;
            Debug.Log($"Interaction mode changed to {value}.");
        }
    }

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

    #region Planet "Navigate To" Variables

    private Quaternion _initRotation;
    private Quaternion _destRotation;
    private float _navToDuration = 1.0f;
    private float _navToProgress = 1;

    #endregion

    #region Planet "Select Area" Variables

    private Vector4 _selectionBoundingBox;
    public Vector4 SelectionBoundingBox {
        get {
            return _selectionBoundingBox;
        }
    }

    private byte _selectionIndex = 0;

    private LineRenderer _lonSelectionStartIndicator;
    private LineRenderer _latSelectionStartIndicator;
    private LineRenderer _lonSelectionEndIndicator;
    private LineRenderer _latSelectionEndIndicator;

    #endregion

    #region Event Handlers

    public override void OnGripDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        if (Vector3.Distance(sender.transform.position, point) > _maxGrabDistance) {
            return;
        }
        _grabber = sender;
        _grabPoint = point;
        _grabRadius = Vector3.Distance(transform.position, point); // This should not change until another grab is made.
        _grabbed = true;
        Debug.Log("Grabbed something at " + point);

        _grabber.cursor.transform.localScale *= 2;
    }

    public override void OnGripUp(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        Ungrab();
    }

    public override void OnTriggerDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {

        if (_interactionMode == XRInteractablePlanetMode.Navigate) {
            Camera eye = sender.cameraRig.GetComponentInChildren<Camera>();
            NavigateTo(point - transform.position, eye.transform.position);
        }

        else if (_interactionMode == XRInteractablePlanetMode.Select) {

            Vector3 direction = transform.InverseTransformPoint(point);
            Vector3 flattened = new Vector3(direction.x, 0, direction.z);
            float angle;

            // Longitude selection
            if (_selectionIndex % 2 == 0) {

                angle = Vector3.Angle(flattened, -Vector3.forward) - 180;

                if (Vector3.Cross(flattened, Vector3.up).y > 0) {
                    angle = -angle;
                }
                Debug.Log($"Lon selection: {angle} degrees");
            }

            // Latitude selection
            else {
                angle = Vector3.Angle(direction, flattened);
                if (direction.y < 0) {
                    angle = -angle;
                }
                Debug.Log($"Lat selection: {angle} degrees");
            }

            _selectionBoundingBox[_selectionIndex] = angle;

            GetCurrentSelectionIndicator().startWidth = CoordinateIndicatorThickness;
            _selectionIndex++;

            // Check if selection is finished.
            if (_selectionIndex == 4) {
                Debug.Log("Selection Complete: " + _selectionBoundingBox);
                TerrainMeshController terrainMeshController = TerrainMeshController.Instance;
                TerrainMesh terrainMesh = terrainMeshController.CreatePartial(_selectionBoundingBox, null);
                terrainMeshController.ShowTerrainMesh(terrainMesh, true);
                ExitSelectionMode();
            }
            else {
                LineRenderer nextCoordinateIndicator = GetCurrentSelectionIndicator();
                nextCoordinateIndicator.startWidth = CoordinateIndicatorActiveThickness;
                nextCoordinateIndicator.enabled = true;
            }
        }
    }

    public override void OnCursorOver(CustomControllerBehavior sender, Vector3 point) {
        if (_interactionMode == XRInteractablePlanetMode.Select) {
            LineRenderer currentCoordinateIndicator = GetCurrentSelectionIndicator();

            Vector3 direction = transform.InverseTransformPoint(point);
            Vector3 flattened = new Vector3(direction.x, 0, direction.z);

            // Longitude selection
            if (_selectionIndex % 2 == 0) {
                float angle = Vector3.Angle(flattened, -Vector3.forward) - 180;
                if (Vector3.Cross(flattened, Vector3.up).y > 0) {
                    angle = -angle;
                }
                currentCoordinateIndicator.transform.localEulerAngles = new Vector3(0, -angle, 0);
            }

            // Latitude selection
            else {
                float angle = Vector3.Angle(direction, flattened);
                if (direction.y < 0) {
                    angle = -angle;
                }
                Vector2 offsetAndScale = CalculateLatitudeIndicatorOffsetAndScale(angle);
                float adjustedScale = offsetAndScale.x * 3.39f + 0.01f;
                currentCoordinateIndicator.transform.localPosition = new Vector3(0, 3.39f * offsetAndScale.y, 0);
                currentCoordinateIndicator.transform.localScale = adjustedScale * Vector3.one;
                //currentCoordinateIndicator.transform.localScale = new Vector3(adjustedScale, 1, adjustedScale);
            }
        }
    }

    #endregion

    #region Unity lifecycle methods

    private void Awake() {

        // Create the latitude and longitude selection indicators.
        GameObject selectionIndicatorsContainer = new GameObject();
        selectionIndicatorsContainer.transform.SetParent(transform, false);
        selectionIndicatorsContainer.name = GameObjectName.PlanetSelectionIndicatorContainer;

        GameObject lonSelectionStartIndicator = new GameObject();
        lonSelectionStartIndicator.transform.SetParent(selectionIndicatorsContainer.transform, false);
        lonSelectionStartIndicator.name = "Lon" + GameObjectName.PlanetSelectionIndicator + "1";
        lonSelectionStartIndicator.transform.localScale = (3.39f + 0.01f) * Vector3.one; // TODO Un-hardcode the radius.
        _lonSelectionStartIndicator = InitCoordinateIndicator(lonSelectionStartIndicator);
        _lonSelectionStartIndicator.enabled = false;
        GeneratePointsForLongitudeIndicator(_lonSelectionStartIndicator);

        GameObject latSelectionStartIndicator = new GameObject();
        latSelectionStartIndicator.transform.SetParent(selectionIndicatorsContainer.transform, false);
        latSelectionStartIndicator.name = "Lat" + GameObjectName.PlanetSelectionIndicator + "1";
        _latSelectionStartIndicator = InitCoordinateIndicator(latSelectionStartIndicator);
        _latSelectionStartIndicator.enabled = false;
        GeneratePointsForLatitudeIndicator(_latSelectionStartIndicator);

        GameObject lonSelectionEndIndicator = new GameObject();
        lonSelectionEndIndicator.transform.SetParent(selectionIndicatorsContainer.transform, false);
        lonSelectionEndIndicator.name = "Lon" + GameObjectName.PlanetSelectionIndicator + "2";
        lonSelectionEndIndicator.transform.localScale = (3.39f + 0.01f) * Vector3.one; // TODO Un-hardcode the radius.
        _lonSelectionEndIndicator = InitCoordinateIndicator(lonSelectionEndIndicator);
        _lonSelectionEndIndicator.enabled = false;
        GeneratePointsForLongitudeIndicator(_lonSelectionEndIndicator);

        GameObject latSelectionEndIndicator = new GameObject();
        latSelectionEndIndicator.transform.SetParent(selectionIndicatorsContainer.transform, false);
        latSelectionEndIndicator.name = "Lat" + GameObjectName.PlanetSelectionIndicator + "2";
        _latSelectionEndIndicator = InitCoordinateIndicator(latSelectionEndIndicator);
        _latSelectionEndIndicator.enabled = false;
        GeneratePointsForLatitudeIndicator(_latSelectionEndIndicator);

    }

    // Use this for initialization
    void Start () {
        ResetSelectionBoundingBox();
	}
	
	// Update is called once per frame
	void Update () {
        if (_navToProgress < 1) {
            _navToProgress += Time.deltaTime / _navToDuration;
            transform.rotation = Quaternion.Lerp(_initRotation, _destRotation, _navToProgress);
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

            }
        }
    }

    #endregion

    #region Naviation methods

    public void NavigateTo(Vector2 coords, Vector3 cameraPosition) {
        // TODO Find a better way to do this without having to
        // rotate the planet to get the new direction.
        Quaternion originalRotation = transform.rotation;
        transform.Rotate(-coords.x, -coords.y, 0, Space.Self);
        Vector3 direction = transform.forward;
        transform.rotation = originalRotation;
        NavigateTo(direction, cameraPosition);
    }

    public void NavigateTo(Vector3 localDirection, Vector3 cameraPosition) {
        Vector3 axis = Vector3.Cross(localDirection, cameraPosition - transform.position);
        float angle = Vector3.Angle(localDirection, cameraPosition - transform.position);
        NavigateTo(angle, axis, cameraPosition);
    }

    private void NavigateTo(float angle, Vector3 axis, Vector3 cameraPosition) {
        
        // Set the initial rotation to the planet's current rotation.
        _initRotation = transform.rotation;

        // Rotate the planet by the specified angle along the specified axis.
        // After rotation, the desired navigation point should be facing the
        // camera, but the orientation of the planet is not guaranteed.
        transform.Rotate(axis, angle, Space.World);

        // Orient the planet relative to the camera position such that the 
        // planet's up direction is aligned with the world's up direction.
        // This will be the destination rotation for the navigation.
        _destRotation = GetOrientedRotation(cameraPosition - transform.position);

        // Reset the planet's rotation.
        transform.rotation = _initRotation;

        // Set the navigation progress counter to zero to start the navigation process.
        _navToProgress = 0;

    }

    /// <summary>
    /// Calculates a version of the planet's current rotation but with the  
    /// planet's up direction oriented toward the world's up direction when 
    /// viewed from the provided relative position.
    /// </summary>
    /// <param name="relativePosition">
    /// A vector describing a position relative to the planet's center.
    /// This is typically the vector between the camera's position and planet's position.
    /// </param>
    private Quaternion GetOrientedRotation(Vector3 relativePosition) {

        // Find the projection of the planet's up vector on the relative position plane.
        // The relative position plane has a normal equal to the relative position vector.
        Vector3 projectedUp = Vector3.ProjectOnPlane(transform.up, relativePosition);

        // Find the "up direction" of the relative position plane by projecting the world up vector to the plane.
        // TODO Handle the cases where the relative position vector is up or down.
        Vector3 relativeUp = Vector3.ProjectOnPlane(Vector3.up, relativePosition);

        // Return the planet's rotation rotated by the angle between the 
        // two projected vectors about the relative position vector.
        return Quaternion.FromToRotation(projectedUp, relativeUp) * transform.rotation;

    }

    #endregion

    #region Selection methods

    public void CancelSelection(bool cancelAll = false) {
        if (_interactionMode != XRInteractablePlanetMode.Select) {
            return;
        }
        if (cancelAll) {
            ExitSelectionMode();
        }
        else {
            if (_selectionIndex > 0) {
                _selectionBoundingBox[_selectionIndex--] = float.NaN;
            }
            else {
                _interactionMode = XRInteractablePlanetMode.Navigate;
            }
        }
    }

    private void ResetSelectionBoundingBox() {
        _selectionBoundingBox = new Vector4(float.NaN, float.NaN, float.NaN, float.NaN);
    }

    private void ExitSelectionMode() {
        ResetSelectionBoundingBox();
        _selectionIndex = 0;
        _lonSelectionStartIndicator.enabled = false;
        _latSelectionStartIndicator.enabled = false;
        _lonSelectionEndIndicator.enabled = false;
        _latSelectionEndIndicator.enabled = false;
        _interactionMode = XRInteractablePlanetMode.Navigate;
    }

    private LineRenderer GetCurrentSelectionIndicator() {
        if (_interactionMode != XRInteractablePlanetMode.Select) {
            return null;
        }
        switch (_selectionIndex) {
            case 0:
                return _lonSelectionStartIndicator;
            case 1:
                return _latSelectionStartIndicator;
            case 2:
                return _lonSelectionEndIndicator;
            case 3:
                return _latSelectionEndIndicator;
            default:
                return null;
        }
    }

    #endregion

    #region Coordinate indicator methods

    private LineRenderer InitCoordinateIndicator(GameObject gameObject, bool loop = true) {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.startWidth = CoordinateIndicatorThickness;
        lineRenderer.loop = loop;
        return lineRenderer;
    }

    private void GeneratePointsForLongitudeIndicator(LineRenderer lineRenderer, float longitude = float.NaN) {
        lineRenderer.positionCount = CoordinateIndicatorSegmentCount;
        float angleIncrement = 2 * Mathf.PI / CoordinateIndicatorSegmentCount;
        for (int i = 0; i < CoordinateIndicatorSegmentCount; i++) {
            float angle = i * angleIncrement;
            Vector3 basePosition = new Vector3(0, Mathf.Sin(angle), Mathf.Cos(angle));
            if (float.IsNaN(longitude)) {
                lineRenderer.SetPosition(i, basePosition);
            }
            else {
                lineRenderer.SetPosition(i, Quaternion.AngleAxis(longitude, Vector3.up) * basePosition);
            }
        }
    }

    private void GeneratePointsForLatitudeIndicator(LineRenderer lineRenderer, float latitude = float.NaN) {
        lineRenderer.positionCount = CoordinateIndicatorSegmentCount;
        float angleIncrement = 2 * Mathf.PI / CoordinateIndicatorSegmentCount;
        Vector2 offsetAndScale = CalculateLatitudeIndicatorOffsetAndScale(float.IsNaN(latitude) ? 0.0f : latitude);
        for (int i = 0; i < CoordinateIndicatorSegmentCount; i++) {
            float angle = i * angleIncrement;
            Vector3 basePosition = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            if (float.IsNaN(latitude)) {
                lineRenderer.SetPosition(i, basePosition);
            }
            else {
                lineRenderer.SetPosition(i, offsetAndScale.x * basePosition + offsetAndScale.y * Vector3.one);
            }
        }
    }

    private Vector2 CalculateLatitudeIndicatorOffsetAndScale(float latitude) {
        latitude *= Mathf.Deg2Rad;
        return new Vector2(
            Mathf.Cos(latitude),    // Horizontal scale
            Mathf.Sin(latitude)     // Vertical offset
        );
    }

    #endregion

    /// <summary>
    /// This function uses the law of cosines formula to find the distance
    /// between the controller and the current location of the grab point.
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
            _grabber.cursor.transform.localScale /= 2;
            _grabber = null;
        }
    }

}
