using UnityEngine;

namespace TrekVRApplication {

    public abstract class XRInteractableObject : MonoBehaviour {

        // TODO Refactor these...
        public bool triggerDoubleClick = true;
        public bool triggerDown = true;
        public bool triggerUp = true;
        public bool padClick = true;
        public bool padTouch = true;
        public bool gripDown = true;
        public bool gripUp = true;
        public bool cursorOver = true;

        public virtual void OnTriggerDoubleClick(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnTriggerUp(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnGripDown(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnGripUp(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnCursorOver(XRController sender, RaycastHit hit) { }

        public virtual void OnCursorEnter(XRController sender, RaycastHit hit) { }

        public virtual void OnCursorLeave(XRController sender, RaycastHit hit) { }

        public virtual void OnPadDown(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnPadUp(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnPadTouch(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnPadUntouch(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

        public virtual void OnPadSwipe(XRController sender, RaycastHit hit, ClickedEventArgs e) { }

    }
}
