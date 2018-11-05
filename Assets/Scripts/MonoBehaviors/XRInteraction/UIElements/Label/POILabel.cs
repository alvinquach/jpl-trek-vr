using UnityEngine;
using UnityEngine.UI;

public class POILabel : MonoBehaviour {

    private Text _labelText;

    public string Text {
        get {
            return _labelText?.text;
        }
        set {
            if (_labelText) {
                _labelText.text = value;
            }
        }
    }

    void Awake() {
        _labelText = gameObject.GetComponentInChildren<Text>();
    }

}