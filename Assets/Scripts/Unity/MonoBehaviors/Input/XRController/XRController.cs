using UnityEngine;

public abstract class XRController : MonoBehaviour {

    [SerializeField]
    public GameObject cursor;

    [SerializeField]
    protected GameObject _cameraRig;

    public GameObject CameraRig { get { return _cameraRig; } }

    public SteamVR_TrackedController controller { get; private set; }

    private void OnEnable() {
        controller = GetComponent<SteamVR_TrackedController>();
        controller.TriggerClicked += TriggerClickedHandler;
        controller.TriggerUnclicked += TriggerUnclickedHandler;
        controller.PadClicked += PadClickedHandler;
        controller.PadUnclicked += PadUnclickedHandler;
        controller.MenuButtonClicked += MenuButtonClickedHandler;
        controller.Gripped += GrippedHandler;
        controller.Ungripped += UngrippedHandler;
    }

    private void OnDisable() {
        controller.TriggerClicked -= TriggerClickedHandler;
        controller.TriggerUnclicked -= TriggerUnclickedHandler;
        controller.PadClicked -= PadClickedHandler;
        controller.PadUnclicked -= PadUnclickedHandler;
        controller.MenuButtonClicked -= MenuButtonClickedHandler;
        controller.Gripped -= GrippedHandler;
        controller.Ungripped -= UngrippedHandler;
    }

    protected virtual void TriggerClickedHandler(object sender, ClickedEventArgs e) { }
    protected virtual void TriggerUnclickedHandler(object sender, ClickedEventArgs e) { }
    protected virtual void PadClickedHandler(object sender, ClickedEventArgs e) { }
    protected virtual void PadUnclickedHandler(object sender, ClickedEventArgs e) { }
    protected virtual void MenuButtonClickedHandler(object sender, ClickedEventArgs e) { }
    protected virtual void GrippedHandler(object sender, ClickedEventArgs e) { }
    protected virtual void UngrippedHandler(object sender, ClickedEventArgs e) { }

}