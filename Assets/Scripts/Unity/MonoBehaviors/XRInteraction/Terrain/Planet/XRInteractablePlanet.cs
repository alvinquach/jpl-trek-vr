using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class XRInteractablePlanet : XRInteractableObject {

        // TODO Move these somewhere else?
        private const int CoordinateIndicatorSegmentCount = 72;
        private const float CoordinateIndicatorThickness = 1.337e-3f;
        private const float CoordinateIndicatorActiveThickness = 6.9e-3f;
        private const float CoordinateIndicatorRadiusOffset = 3.33e-3f;
        private const int ControllerModalBoundingBoxUpdateInterval = 10;

        public XRInteractablePlanetMode InteractionMode { get; private set; } = XRInteractablePlanetMode.Navigate;

        private Material _coordinateIndicatorMaterial;

        #region Grab variables

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
        private XRController _grabber;

        #endregion

        #region Planet "Navigate To" variables

        private Quaternion _initRotation;
        private Quaternion _destRotation;
        private float _navToDuration = 1.0f;
        private float _navToProgress = 1;

        #endregion

        #region Planet "Select Area" variables

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
        private POILabel _coordSelectionLabel;

        private int _framesSinceLastControllerModalUpdate = 0;

        #endregion

        #region Event handlers

        public override void OnGripDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            if (Vector3.Distance(sender.transform.position, hit.point) > _maxGrabDistance) {
                return;
            }
            _grabber = sender;
            _grabPoint = hit.point;
            _grabRadius = Vector3.Distance(transform.position, hit.point); // This should not change until another grab is made.
            _grabbed = true;
            Debug.Log("Grabbed something at " + hit.point);

            _grabber.cursor.transform.localScale *= 2;
        }

        public override void OnGripUp(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            Ungrab();
        }

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {

            if (InteractionMode == XRInteractablePlanetMode.Navigate) {
                Camera eye = UserInterfaceManager.Instance.XRCamera;
                NavigateTo(hit.point - transform.position, eye.transform.position);
            }

            else if (InteractionMode == XRInteractablePlanetMode.Select) {

                Vector3 direction = transform.InverseTransformPoint(hit.point);
                Vector3 flattened = new Vector3(direction.x, 0, direction.z);
                float angle;

                // Longitude selection
                if (_selectionIndex % 2 == 0) {
                    angle = Vector3.Angle(flattened, -Vector3.forward) - 180;
                    if (Vector3.Cross(flattened, Vector3.forward).y > 0) {
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
                    TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
                    TerrainModel terrainModel = terrainModelManager.CreatePartial(_selectionBoundingBox, null);
                    terrainModelManager.ShowTerrainModel(terrainModel, false);
                    ExitSelectionMode();
                }
                else {
                    LineRenderer nextCoordinateIndicator = GetCurrentSelectionIndicator();
                    nextCoordinateIndicator.startWidth = CoordinateIndicatorActiveThickness;
                    nextCoordinateIndicator.enabled = true;
                }

                SendBoundingBoxUpdateToControllerModal(new BoundingBox(_selectionBoundingBox));
            }
        }

        public override void OnCursorOver(XRController sender, RaycastHit hit) {

            /*
             * Under select mode, there are currently two things that happen when the cursor
             * is pointed somewhere within the planet model:
             *
             * 1. A line indicating where current longitude or latitude is, referred to as a
             * coordinate indicator, moves along with the cursor position.
             *
             * 2. A text overlay at the cursor location indicatest the current longitude or 
             * latitude angle.
             */
            if (InteractionMode == XRInteractablePlanetMode.Select) {

                // Update the position and angle of the coordinate selection label.
                _coordSelectionLabel.transform.position = hit.point;
                _coordSelectionLabel.transform.forward = -hit.normal;

                // Get the current coordinate indicator to set its angles.
                LineRenderer currentCoordinateIndicator = GetCurrentSelectionIndicator();

                Vector3 direction = transform.InverseTransformPoint(hit.point);
                Vector3 flattened = new Vector3(direction.x, 0, direction.z);

                float angle;

                // Longitude selection
                if (_selectionIndex % 2 == 0) {
                    angle = Vector3.Angle(flattened, -Vector3.forward) - 180;
                    if (Vector3.Cross(flattened, Vector3.forward).y > 0) {
                        angle = -angle;
                    }
                    currentCoordinateIndicator.transform.localEulerAngles = new Vector3(0, -angle, 0);

                    _coordSelectionLabel.Text = $"Lon: {angle.ToString("0.00")}°";
                }

                // Latitude selection
                else {
                    angle = Vector3.Angle(direction, flattened);
                    if (direction.y < 0) {
                        angle = -angle;
                    }
                    Vector2 offsetAndScale = CalculateLatitudeIndicatorOffsetAndScale(angle);
                    float modelRadius = Mars.Radius * GlobalTerrainModel.GlobalModelScale;

                    currentCoordinateIndicator.transform.localPosition = new Vector3(0, modelRadius * offsetAndScale.y, 0);
                    currentCoordinateIndicator.transform.localScale = (offsetAndScale.x * modelRadius + CoordinateIndicatorRadiusOffset) * Vector3.one;

                    _coordSelectionLabel.Text = $"Lat: {angle.ToString("0.00")}°";
                }

                // Send updated to controller modal.
                if (_framesSinceLastControllerModalUpdate >= ControllerModalBoundingBoxUpdateInterval) {
                    BoundingBox bbox = new BoundingBox(_selectionBoundingBox);
                    bbox[_selectionIndex] = angle;
                    SendBoundingBoxUpdateToControllerModal(bbox);
                } 
                _framesSinceLastControllerModalUpdate++;
            }
        }

        #endregion

        #region Unity lifecycle methods

        private void Awake() {

            float indicatorRadius = Mars.Radius * GlobalTerrainModel.GlobalModelScale + CoordinateIndicatorRadiusOffset;

            // Create material for coordinate indicators
            _coordinateIndicatorMaterial = new Material(Shader.Find("Unlit/Color"));
            _coordinateIndicatorMaterial.SetColor("_Color", new Color32(0, 224, 255, 255));

            // Create the latitude and longitude selection indicators.
            GameObject selectionIndicatorsContainer = new GameObject();
            selectionIndicatorsContainer.transform.SetParent(transform, false);
            selectionIndicatorsContainer.name = GameObjectName.PlanetSelectionIndicatorContainer;

            GameObject lonSelectionStartIndicator = new GameObject();
            lonSelectionStartIndicator.transform.SetParent(selectionIndicatorsContainer.transform, false);
            lonSelectionStartIndicator.name = "Lon" + GameObjectName.PlanetSelectionIndicator + "1";
            lonSelectionStartIndicator.transform.localScale = indicatorRadius * Vector3.one; // TODO Un-hardcode the radius.
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
            lonSelectionEndIndicator.transform.localScale = indicatorRadius * Vector3.one; // TODO Un-hardcode the radius.
            _lonSelectionEndIndicator = InitCoordinateIndicator(lonSelectionEndIndicator);
            _lonSelectionEndIndicator.enabled = false;
            GeneratePointsForLongitudeIndicator(_lonSelectionEndIndicator);

            GameObject latSelectionEndIndicator = new GameObject();
            latSelectionEndIndicator.transform.SetParent(selectionIndicatorsContainer.transform, false);
            latSelectionEndIndicator.name = "Lat" + GameObjectName.PlanetSelectionIndicator + "2";
            _latSelectionEndIndicator = InitCoordinateIndicator(latSelectionEndIndicator);
            _latSelectionEndIndicator.enabled = false;
            GeneratePointsForLatitudeIndicator(_latSelectionEndIndicator);

            // Instantiate a copy of the coordinate template to display the coordiate values.
            GameObject coordinateTemplate = TemplateService.Instance.GetTemplate(GameObjectName.CoordinateTemplate);
            if (coordinateTemplate) {
                GameObject copy = Instantiate(coordinateTemplate);
                copy.transform.SetParent(selectionIndicatorsContainer.transform); // TODO Move this to a container for labels.
                _coordSelectionLabel = copy.transform.GetComponent<POILabel>();
            }

        }

        // Use this for initialization
        void Start() {
            ResetSelectionBoundingBox();
        }

        // Update is called once per frame
        void Update() {
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

        public void SwitchToMode(XRInteractablePlanetMode mode) {
            if (InteractionMode == mode) {
                return;
            }

            // Switching away from current mode.
            switch (InteractionMode) {
                case XRInteractablePlanetMode.Select:
                    CancelSelection(true);
                    break;
                case XRInteractablePlanetMode.Disabled:
                    GlobalTerrainModel globe = GetComponent<GlobalTerrainModel>();
                    globe.UseEnabledMaterial();
                    break;
            }

            // Switch to new mode.
            switch (mode) {
                case XRInteractablePlanetMode.Select:
                    _lonSelectionStartIndicator.startWidth = CoordinateIndicatorActiveThickness;
                    _lonSelectionStartIndicator.enabled = true;
                    _coordSelectionLabel.gameObject.SetActive(true);
                    break;
                case XRInteractablePlanetMode.Disabled:
                    GlobalTerrainModel globe = GetComponent<GlobalTerrainModel>();
                    globe.UseDisabledMaterial();
                    break;
            }

            InteractionMode = mode;
            Debug.Log($"Interaction mode changed to {mode}.");
        }

        #region Naviation methods

        public void NavigateTo(Vector2 latLon, Vector3 cameraPosition) {

            // Calculate vector representing the latitude and longitude.
            // This is a vector pointing from the cetner of the planet towards
            // the surface where the coordinate is located.
            Vector3 latLonDirection = CoordinateUtils.LatLonToDirection(latLon);

            // Convert the lat-long direction vector to a vector that is relative
            // to the current orientation/rotation of the planet.
            Vector3 direction = transform.TransformDirection(latLonDirection);

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
            if (InteractionMode != XRInteractablePlanetMode.Select) {
                return;
            }
            if (cancelAll) {
                ExitSelectionMode(true);
            }
            else {
                if (_selectionIndex > 0) {
                    _selectionBoundingBox[_selectionIndex--] = float.NaN;
                }
                else {
                    InteractionMode = XRInteractablePlanetMode.Navigate;
                    ExitSelectionMode(true);
                }
            }
        }

        private void ResetSelectionBoundingBox() {
            _selectionBoundingBox = new Vector4(float.NaN, float.NaN, float.NaN, float.NaN);
        }

        private void ExitSelectionMode(bool openMainModal = false) {
            ResetSelectionBoundingBox();
            _selectionIndex = 0;
            _lonSelectionStartIndicator.enabled = false;
            _latSelectionStartIndicator.enabled = false;
            _lonSelectionEndIndicator.enabled = false;
            _latSelectionEndIndicator.enabled = false;
            _coordSelectionLabel.gameObject.SetActive(false);
            InteractionMode = XRInteractablePlanetMode.Navigate;
            UserInterfaceManager.Instance.HideControllerModalsWithActivity(ControllerModalActivity.BBoxSelection);
            if (openMainModal) {
                UserInterfaceManager.Instance.MainModal.Visible = true;
            }
        }

        private LineRenderer GetCurrentSelectionIndicator() {
            if (InteractionMode != XRInteractablePlanetMode.Select) {
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

        private void SendBoundingBoxUpdateToControllerModal(BoundingBox bbox) {
            Browser browser = UserInterfaceManager.Instance
                .GetControllerModalWithActivity(ControllerModalActivity.BBoxSelection)
                .Browser;

            string js =
                $"let component = {AngularComponentContainerPath}.{BoundingBoxSelectionModalName};" +
                $"component && component.updateBoundingBox({bbox.ToString(", ", 7)}, {_selectionIndex});";

            browser.EvalJSCSP(js);
            _framesSinceLastControllerModalUpdate = 0;
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
            lineRenderer.material = _coordinateIndicatorMaterial;
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

}
