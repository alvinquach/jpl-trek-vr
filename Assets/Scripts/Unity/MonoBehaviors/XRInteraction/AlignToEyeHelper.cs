using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     The game object that this script is attached to will always face the eye (camera).
    /// </summary>
    // TODO Rename this script
    public class AlignToEyeHelper : MonoBehaviour {

        private Camera _eye;

        /// <summary>
        ///     Whether to also apply the upwards alignation of the camera to the game object.
        /// </summary>
        public bool alignUpDirection = true;

        void Start() {
            _eye = UserInterfaceManager.Instance.XRCamera;
        }

        void Update() {
            if (alignUpDirection) {
                transform.rotation = _eye.transform.rotation;
            } else {
                transform.forward = _eye.transform.forward;
            }
        }

    }

}