using UnityEngine;

namespace TrekVRApplication {

    public class FlashlightController : MonoBehaviour {

        private Color[] _availableColors = new Color[] {
            Color.white,
            Color.red,
            Color.green,
            Color.blue,
            Color.cyan,
            Color.magenta,
            Color.yellow
        };

        [SerializeField]
        private MeshRenderer _bulbModel;

        [SerializeField]
        private Material _offMaterial;

        [SerializeField]
        private Material _onMaterial;

        [SerializeField]
        [Tooltip("Wheter the flashlight is turned on at start.")]
        private bool _startOn = false;

        private bool _state = false;

        private int _selectedColorIndex = 0;

        void Awake() {
            _onMaterial = new Material(_onMaterial);
            if (_startOn) {
                TurnOn();
            }
            else {
                TurnOff();
            }
            SetColor(_availableColors[_selectedColorIndex]);
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
            foreach (Light light in transform.GetComponentsInChildren<Light>()) {
                light.enabled = true;
            }
            if (_bulbModel && _onMaterial) {
                _bulbModel.material = _onMaterial;
            }
            _state = true;
        }

        public void TurnOff() {
            // TODO Check for null
            foreach (Light light in transform.GetComponentsInChildren<Light>()) {
                light.enabled = false;
            }
            if (_bulbModel && _offMaterial) {
                _bulbModel.material = _offMaterial;
            }
            _state = false;
        }

        public void CycleNextColor() {
            _selectedColorIndex++;
            if (_selectedColorIndex >= _availableColors.Length) {
                _selectedColorIndex = 0;
            }
            SetColor(_availableColors[_selectedColorIndex]);
        }

        /// <summary>
        ///     Sets the flashlight color.
        /// </summary>
        /// <param name="rgb">RGB value range from 0.0 to 1.0.</param>
        private void SetColor(Color color) {
            color.a = 1.0f;
            foreach (Light light in transform.GetComponentsInChildren<Light>()) {
                light.color = Color.Lerp(Color.white, color, 0.69f);
            }
            _onMaterial.SetColor("_EmissionColor", color);
            _onMaterial.SetColor("_Color", Color.Lerp(Color.white, color, 0.69f));
        }

    }

}