using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class XRInteractableObject : MonoBehaviour {

    public bool triggerClick = true;
    public bool triggerDoubleClick = true;
    public bool triggerDown = true;
    public bool triggerUp = true;
    public bool gripDown = true;
    public bool gripUp = true;
    public bool cursorOver = true;

    public virtual void OnTriggerClick(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) { }

    public virtual void OnTriggerDoubleClick(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) { }

    public virtual void OnTriggerDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) { }

    public virtual void OnTriggerUp(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) { }

    public virtual void OnGripDown(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) { }

    public virtual void OnGripUp(CustomControllerBehavior sender, Vector3 point, ClickedEventArgs e) { }

    public virtual void OnCursorOver(CustomControllerBehavior sender, Vector3 point) { }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
