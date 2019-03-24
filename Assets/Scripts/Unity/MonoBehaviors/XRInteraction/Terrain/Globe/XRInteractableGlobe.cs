using UnityEngine;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class XRInteractableGlobe : XRInteractableObject {

        public XRInteractableGlobeMode InteractionMode { get; private set; } = XRInteractableGlobeMode.Navigate;

        private XRInteractableGlobeCoordinateLinesController _coordinateLinesController;

        private XRInteractableGlobeBoundingBoxSelectionController _bboxSelectionController;

        private GlobeTerrainModel _globeModel;

        #region Grab variables

        [SerializeField]
        private float _maxGrabDistance = 10.0f;

        [SerializeField]
        private float _angularDeceleration = 0.1f;

        [SerializeField]
        private float _linearDeceleration = 0.1f;

        [SerializeField]
        private int _decelerationSmoothing = 10;

        private XRController _grabber;

        private bool _triggerGrabbed = false;
        private Vector3 _grabPoint;
        private float _grabRadius;

        private bool _gripGrabbed = false;
        private Quaternion _grabberRotation;

        #endregion

        #region Planet "Navigate To" variables

        private Quaternion _initRotation;
        private Quaternion _destRotation;
        private float _navToDuration = 1.0f;
        private float _navToProgress = 1;

        #endregion

        #region Event handlers

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            if (InteractionMode == XRInteractableGlobeMode.Navigate) {
                if (Vector3.Distance(sender.transform.position, hit.point) > _maxGrabDistance || _gripGrabbed) {
                    return;
                }
                _grabber = sender;
                _grabPoint = hit.point;
                _grabRadius = Vector3.Distance(transform.position, hit.point); // This should not change until another grab is made.
                _triggerGrabbed = true;
                _grabber.LaserPointer.Active = true;
            }
            else if (InteractionMode == XRInteractableGlobeMode.Select) {
                _bboxSelectionController.MakeBoundarySelection(hit);
            }
        }

        public override void OnTriggerUp(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            TriggerUngrab();
        }

        public override void OnTriggerDoubleClick(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            if (InteractionMode == XRInteractableGlobeMode.Navigate) {
                Camera eye = UserInterfaceManager.Instance.XRCamera;
                NavigateTo(hit.point - transform.position, eye.transform.position);
            }
        }

        public override void OnGripDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            if (Vector3.Distance(sender.transform.position, hit.point) > _maxGrabDistance || _triggerGrabbed) {
                return;
            }
            _grabber = sender;
            _grabberRotation = _grabber.transform.rotation;
            _gripGrabbed = true;
            _grabber.LaserPointer.Visible = false;
            _grabber.OnUngripped += GripUngrab;
        }

        public override void OnCursorOver(XRController sender, RaycastHit hit) {
            if (InteractionMode == XRInteractableGlobeMode.Select) {
                _bboxSelectionController.UpdateCursorPosition(hit);
            }
        }

        #endregion

        #region Unity lifecycle methods

        private void Awake() {

            // Create the latitude and longitude selection indicators.
            GameObject selectionIndicatorsContainer = new GameObject(GameObjectName.PlanetSelectionIndicatorContainer) {
                layer = (int)CullingLayer.Terrain // TODO Make a new layer for coordinate lines and labels
            };
            selectionIndicatorsContainer.transform.SetParent(transform, false);
            _bboxSelectionController = selectionIndicatorsContainer.AddComponent<XRInteractableGlobeBoundingBoxSelectionController>();
            _bboxSelectionController.SetEnabled(false);

            // Add container for coordinate lines
            GameObject coordinateLines = new GameObject(GameObjectName.PlanetStaticCoordinateLines) {
                layer = (int)CullingLayer.Terrain // TODO Make a new layer for coordinate lines and labels
            };
            coordinateLines.transform.SetParent(transform, false);
            _coordinateLinesController = coordinateLines.AddComponent<XRInteractableGlobeCoordinateLinesController>();
            _coordinateLinesController.SetVisible(false);

        }

        // Use this for initialization
        void Start() {
            if (!_globeModel) {
                _globeModel = GetComponent<GlobeTerrainModel>();

                // There is no way to unsubscribe from this...but unsubscribing
                // is not really necessary in this case.
                _globeModel.OnInitComplete += () => {
                    SetCoordinateLinesVisiblity(true);
                };
            }
            _bboxSelectionController.ResetSelectionBoundingBox();
        }

        // Update is called once per frame
        void Update() {

            if (_navToProgress < 1) {
                _navToProgress += Time.deltaTime / _navToDuration;
                transform.rotation = Quaternion.Lerp(_initRotation, _destRotation, _navToProgress);
            }

            if (_triggerGrabbed && _grabber) {

                // Ray representing the forward direction of the controller.
                Ray forward = new Ray(_grabber.transform.position, _grabber.transform.forward);

                // Check if the controller is pointing outside of the planet's bounds.
                // If the controller is pointing outside of the planet, then ungrab the planet.
                float forwardDistFromObject = MathUtils.DistancePointLine(transform.position, _grabber.transform.position, forward.GetPoint(_maxGrabDistance));
                if (forwardDistFromObject > _grabRadius) {
                    Debug.Log("Lost grip on grabbed object (distance too large, " + forwardDistFromObject + ">" + _grabRadius + ")");
                    TriggerUngrab();
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

            if (_gripGrabbed && _grabber) {
                Quaternion controllerRotation = _grabber.transform.rotation;

                // https://forum.unity.com/threads/get-the-difference-between-two-quaternions-and-add-it-to-another-quaternion.513187/
                Quaternion rotation = controllerRotation * Quaternion.Inverse(_grabberRotation);
                transform.rotation = rotation * transform.rotation;
                _grabberRotation = controllerRotation;
            }

        }

        #endregion

        public void SwitchToMode(XRInteractableGlobeMode mode) {
            if (InteractionMode == mode) {
                return;
            }

            // Switching away from current mode.
            switch (InteractionMode) {
                case XRInteractableGlobeMode.Select:
                    CancelSelection(true, mode);
                    break;
                case XRInteractableGlobeMode.Disabled:
                    GetComponent<SphereCollider>().enabled = true;
                    SetCoordinateLinesVisiblity(true);
                    break;
            }

            // Switch to new mode.
            switch (mode) {
                case XRInteractableGlobeMode.Select:
                    _bboxSelectionController.SetEnabled(true);
                    break;
                case XRInteractableGlobeMode.Disabled:
                    GetComponent<SphereCollider>().enabled = false;
                    SetCoordinateLinesVisiblity(false);
                    break;
            }

            Debug.Log($"SWITCHING MODES: {InteractionMode} --> {mode}");
            InteractionMode = mode;
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

        public void CancelSelection(bool cancelAll = false, XRInteractableGlobeMode nextMode = XRInteractableGlobeMode.Navigate) {
            if (InteractionMode != XRInteractableGlobeMode.Select) {
                return;
            }
            if (_bboxSelectionController.CancelSelection(cancelAll)) {
                InteractionMode = nextMode;
                _bboxSelectionController.SetEnabled(false);
                UserInterfaceManager.Instance.MainModal.Visible = true;
            }
        }

        public void SetCoordinateLinesVisiblity(bool visible) {
            _coordinateLinesController.SetVisible(visible);
        }

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

        private void TriggerUngrab() {
            if (_grabber != null) {
                _triggerGrabbed = false;
                _grabber.LaserPointer.Active = false;
                _grabber = null;
            }
        }

        private void GripUngrab(object sender, ClickedEventArgs e) {
            if (_grabber != null) {
                _gripGrabbed = false;
                _grabber.LaserPointer.Visible = true;
                _grabber.OnUngripped -= GripUngrab;
                _grabber = null;
            }
        }

    }

}
