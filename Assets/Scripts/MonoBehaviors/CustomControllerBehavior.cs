using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEditor;

public class CustomControllerBehavior : MonoBehaviour {

    [SerializeField]
    private float _maxInteractionDistance = 20.0f;

    [SerializeField]
    public GameObject cursor;

    [SerializeField]
    private GameObject _cameraRig;

    [SerializeField]
    private float _speedMultiplier = 0.1f;

    public SteamVR_TrackedController controller { get; private set; }

    private bool _padClicked = false;

    private void OnEnable() {
        controller = GetComponent<SteamVR_TrackedController>();
        controller.TriggerClicked += TriggerClickedHandler;
        controller.TriggerUnclicked += TriggerUnclickedHandler;
        controller.PadClicked += PadClickedHandler;
        controller.PadUnclicked += PadUnclickedHandler;
    }

    private void OnDisable() {
        controller.TriggerClicked -= TriggerClickedHandler;
        controller.TriggerUnclicked -= TriggerUnclickedHandler;
        controller.PadClicked -= PadClickedHandler;
        controller.PadUnclicked -= PadUnclickedHandler;
    }

    #region Controller Event Handlers

    private void TriggerClickedHandler(object sender, ClickedEventArgs e) {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _maxInteractionDistance)) {
            XRInteractableObject obj = hit.transform.GetComponent<XRInteractableObject>();
            if (obj != null) {
                // TODO Verify sender class.
                obj.OnTriggerDown(this, hit.point, e);
            }
        }
    }

    private void TriggerUnclickedHandler(object sender, ClickedEventArgs e) {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _maxInteractionDistance)) {
            XRInteractableObject obj = hit.transform.GetComponent<XRInteractableObject>();
            if (obj != null) {
                // TODO Verify sender class.
                obj.OnTriggerUp(this, hit.point, e);
            }
        }
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

        // Update player movement
        if (_padClicked) {

            // Get the pad position from the controller device.
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)controller.controllerIndex);
            Vector2 axis = device.GetAxis();

            // Move the player based on controller direction and pad position.
            // Movement is limited along the xz-plane.
            Vector3 direction = Vector3.Scale(transform.forward, new Vector3(1, 0, 1));
            _cameraRig.transform.position += (axis.y > 0 ? 1 : -1) * _speedMultiplier * direction;

        }

    }

}