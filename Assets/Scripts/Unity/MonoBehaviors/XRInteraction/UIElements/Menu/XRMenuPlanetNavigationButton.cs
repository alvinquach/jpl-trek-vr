using UnityEngine;

namespace TrekVRApplication {

    public class XRMenuPlanetNavigationButton : XRMenuElement {

        [SerializeField]
        private XRInteractablePlanet _planet;

        public float latitude;

        public float longitude;

        //public override void OnTriggerDoubleClick(PrimaryXRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) {
        public override void OnTriggerDown(XRController sender, Vector3 point, Vector3 normal, ClickedEventArgs e) {

            if (_planet != null) {
                Camera eye = sender.CameraRig.GetComponentInChildren<Camera>();
                _planet.NavigateTo(new Vector2(latitude, longitude), eye.transform.position);
            }

        }

    }

}
