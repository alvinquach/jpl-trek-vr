using UnityEngine;

public class FlashlightController : MonoBehaviour {

    [SerializeField]
    private Light _light;

    [SerializeField]
    private GameObject _offModel;

    [SerializeField]
    private GameObject _onModel;

    [SerializeField]
    [Tooltip("Wheter the flashlight is turned on at start.")]
    private bool _startOn = false;

    private bool _state = false;

    void Awake() {
        if (_startOn) {
            TurnOn();
        }
        else {
            TurnOff();
        }
    }

    public void Toggle() {
        if (_state) {
            TurnOff();
        }
        else {
            TurnOn();
        }
    }

    public void TurnOn() {
        // TODO Check for null
        _light.enabled = true;
        _onModel.SetActive(true);
        _offModel.SetActive(false);
        _state = true;
    }

    public void TurnOff() {
        // TODO Check for null
        _light.enabled = false;
        _onModel.SetActive(false);
        _offModel.SetActive(true);
        _state = false;
    }

}