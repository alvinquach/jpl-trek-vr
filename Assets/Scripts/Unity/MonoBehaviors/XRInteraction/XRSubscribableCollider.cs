using System;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class XRSubscribableCollider : XRInteractableObject {

        public event Action OnColliderClicked = () => { };

        public XRSubscribableCollider() {
            triggerDoubleClick = false;
            triggerUp = false;
            padClick = false;
            padTouch = false;
            gripDown = false;
            gripUp = false;
            cursorOver = false;
        }

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            OnColliderClicked.Invoke();
        }

    }

}