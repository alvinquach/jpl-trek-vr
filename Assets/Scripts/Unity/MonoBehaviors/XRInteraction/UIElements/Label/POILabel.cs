using UnityEngine;
using UnityEngine.UI;

namespace TrekVRApplication {

    public class POILabel : MonoBehaviour {

        private Text _labelText;

        public string Text {
            get => _labelText?.text;
            set {
                if (!_labelText) {
                    _labelText = gameObject.GetComponentInChildren<Text>();
                }
                _labelText.text = value;
            }
        }

        public Material Material {
            get => _labelText?.material;
            set {
                if (!_labelText) {
                    _labelText = gameObject.GetComponentInChildren<Text>();
                }
                _labelText.material = value;
            }
        }

    }

}