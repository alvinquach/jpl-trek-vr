using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRMenuElement : XRInteractableObject {

    [SerializeField]
    private XRInteractablePlanet _planet;

    public float latitude;

    public float longitude;

    //public override void OnTriggerDoubleClick(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
    public override void OnTriggerDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        if (_planet != null) {
            Camera eye = sender.cameraRig.GetComponentInChildren<Camera>();
            _planet.NavigateTo(new Vector2(latitude, longitude), eye.transform.position);
        }
    }

}
