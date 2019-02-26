using UnityEngine;

namespace TrekVRApplication {

    public class PrimaryXRController : XRController {

        /// <summary>
        ///     Whether there was a hit in the current frame.
        /// </summary>
        private bool _hit;

        /// <summary>
        ///     The hit info for the current frame.
        /// </summary>
        private RaycastHit _hitInfo;

        /// <summary>
        ///     Max distance from the controller to detect hits.
        /// </summary>
        [SerializeField]
        private float _maxInteractionDistance = 20.0f;

        private MainModal MainModal {
            get { return UserInterfaceManager.Instance.MainModal; }
        }

        [SerializeField]
        private float _speedMultiplier = 0.1f;

        private bool _padClicked = false;

        private XRInteractableObject _activeObject;

        #region Controller event handlers

        protected override void TriggerClickedHandler(object sender, ClickedEventArgs e) {
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.triggerDown) {
                obj.OnTriggerDown(this, _hitInfo, e);
            }
            base.TriggerClickedHandler(sender, e);
        }

        protected override void TriggerUnclickedHandler(object sender, ClickedEventArgs e) {
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.triggerUp) {
                obj.OnTriggerUp(this, _hitInfo, e);
            }
            base.TriggerUnclickedHandler(sender, e);
        }

        protected override void PadClickedHandler(object sender, ClickedEventArgs e) {
            Debug.Log("Pad clicked at (" + e.padX + ", " + e.padY + ")");
            _padClicked = true;
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.padClick) {
                obj.OnPadDown(this, _hitInfo, e);
            }
            base.PadClickedHandler(sender, e);
        }

        protected override void PadUnclickedHandler(object sender, ClickedEventArgs e) {
            Debug.Log("Pad unclicked at (" + e.padX + ", " + e.padY + ")");
            _padClicked = false;
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.padClick) {
                obj.OnPadUp(this, _hitInfo, e);
            }
            base.PadUnclickedHandler(sender, e);
        }

        protected override void PadTouchedHandler(object sender, ClickedEventArgs e) {
            //Debug.Log("Pad touched at (" + e.padX + ", " + e.padY + ")");
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.padTouch) {
                obj.OnPadTouch(this, _hitInfo, e);
            }
            base.PadTouchedHandler(sender, e);
        }

        protected override void PadUntouchedHandler(object sender, ClickedEventArgs e) {
            //Debug.Log("Pad untouched at (" + e.padX + ", " + e.padY + ")");
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.padTouch) {
                obj.OnPadUntouch(this, _hitInfo, e);
            }
            base.PadUntouchedHandler(sender, e);
        }

        protected override void PadSwipeHandler(object sender, ClickedEventArgs e) {
            //Debug.Log("Pad input received at (" + e.padX + ", " + e.padY + ")");
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.padTouch) {
                obj.OnPadSwipe(this, _hitInfo, e);
            }
            base.PadSwipeHandler(sender, e);
        }

        protected override void GrippedHandler(object sender, ClickedEventArgs e) {
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.gripDown) {
                // TODO Verify sender class.
                obj.OnGripDown(this, _hitInfo, e);
            }
            base.GrippedHandler(sender, e);
        }

        protected override void UngrippedHandler(object sender, ClickedEventArgs e) {
            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj && obj.gripUp) {
                // TODO Verify sender class.
                obj.OnGripUp(this, _hitInfo, e);
            }
            base.UngrippedHandler(sender, e);
        }

        #endregion

        protected override void Update() {

            base.Update();

            // Update player movement
            if (_padClicked) {

                // Get the pad position from the controller device.
                SteamVR_Controller.Device device = SteamVR_Controller.Input((int)Controller.controllerIndex);
                Vector2 axis = device.GetAxis();

                // Move the player based on controller direction and pad position.
                // Movement is limited along the xz-plane.
                Transform cameraRig = UserInterfaceManager.Instance.XRCamera.transform.parent;
                Vector3 direction = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
                cameraRig.position += (axis.y > 0 ? 1 : -1) * _speedMultiplier * direction;

            }

            // Calculate the raycast hit for the current frame.
            RaycastHit hitInfo;
            _hit = Physics.Raycast(transform.position, transform.forward, out hitInfo, _maxInteractionDistance);
            _hitInfo = hitInfo;

            XRInteractableObject obj = GetInteractableObjectIfHit();
            if (obj) {
                if (_activeObject != obj) {

                    // If there was previously a different object being hovered over, then
                    //  we send a OnCursorLeave event to it before setting the new object
                    if (_activeObject) {
                        _activeObject.OnCursorLeave(this, _hitInfo);
                    }

                    if (obj) {
                        obj.OnCursorEnter(this, _hitInfo);
                    }

                    _activeObject = obj;
                }

                if (_activeObject) {
                    _activeObject.OnCursorOver(this, _hitInfo);
                    cursor.transform.position = _hitInfo.point;
                    cursor.SetActive(true);
                }
                else {
                    cursor.SetActive(false);
                }
            }
            else {
                // TODO Send a cursor leave event
                _activeObject = null;
                cursor.SetActive(false);
            }

        }

        private XRInteractableObject GetInteractableObjectIfHit() {
            if (!_hit) {
                return null;
            }
            return _hitInfo.transform.GetComponent<XRInteractableObject>();
        }

    }

}