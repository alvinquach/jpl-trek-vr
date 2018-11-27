using UnityEngine;

namespace TrekVRApplication {

    public abstract class XRInteractableObject : MonoBehaviour {

        public bool triggerClick = true;
        public bool triggerDoubleClick = true;
        public bool triggerDown = true;
        public bool triggerUp = true;
        public bool gripDown = true;
        public bool gripUp = true;
        public bool cursorOver = true;

        public virtual void OnTriggerClick(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) { }

        public virtual void OnTriggerDoubleClick(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) { }

        public virtual void OnTriggerDown(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) { }

        public virtual void OnTriggerUp(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) { }

        public virtual void OnGripDown(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) { }

        public virtual void OnGripUp(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) { }

        public virtual void OnCursorOver(XRController sender, Vector3 point, Vector3 normal) { }

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

    }
}
