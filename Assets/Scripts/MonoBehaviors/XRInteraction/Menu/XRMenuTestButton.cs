using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Button script for testing various functionality.
/// </summary>
public class XRMenuTestButton : XRMenuElement {

    public override void OnTriggerDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) {
        Debug.Log("TESTING");
    }

}
