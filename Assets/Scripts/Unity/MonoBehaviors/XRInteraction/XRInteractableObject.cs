using System;
using UnityEngine;

namespace TrekVRApplication {

    public abstract class XRInteractableObject : MonoBehaviour {

        // TODO Refactor these...
        public bool triggerClick = true;
        public bool triggerDoubleClick = true;
        public bool triggerDown = true;
        public bool triggerUp = true;
        public bool gripDown = true;
        public bool gripUp = true;
        public bool cursorOver = true;

        // TODO Not currently used. Either implement or remove this.
        public virtual void OnTriggerClick(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        // TODO Not currently used. Either implement or remove this.
        public virtual void OnTriggerDoubleClick(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnTriggerUp(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnGripDown(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnGripUp(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnCursorOver(XRController sender, RaycastHit hit) { }

        public virtual void OnCursorEnter(XRController sender, RaycastHit hit) { }

        public virtual void OnCursorLeave(XRController sender, RaycastHit hit) { }

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

    }
}
