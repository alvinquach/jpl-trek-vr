using UnityEngine;
using UnityEngine.Rendering;

namespace TrekVRApplication {

    public class XRControllerLaserPointer : MonoBehaviour {

        [SerializeField]
        private float _laserThickeness = 0.004f;

        [SerializeField]
        private float _laserActiveThickness = 0.01f;

        [SerializeField]
        private float _endpointScale = 0.01f;

        [SerializeField]
        private float _endpointActiveScale = 0.025f;

        [SerializeField]
        private GameObject _cursor;

        [SerializeField]
        private Material _material;

        private LineRenderer _lineRenderer;

        public float MaxDistance { get; set; }

        private float _distance = float.PositiveInfinity;
        public float Distance {
            get {
                return _distance;
            }
            set {
                _distance = Mathf.Clamp(value, 0, MaxDistance);
                Vector3 point = _distance * Vector3.forward;
                _lineRenderer.SetPosition(1, point);
                _cursor.transform.localPosition = point;
                _cursor.SetActive(_visible && value <= MaxDistance);
            }
        }

        private bool _active = false;
        public bool Active {
            get {
                return _active;
            }
            set {
                _cursor.transform.localScale = (value ? _endpointActiveScale : _endpointScale) * Vector3.one;
                _lineRenderer.startWidth = value ? _laserActiveThickness : _laserThickeness;
                _active = value;
            }
        }

        private bool _visible = true;
        public bool Visible {
            get {
                return _visible;
            }
            set {
                _lineRenderer.enabled = value;
                _cursor.SetActive(value);
                _visible = value;
            }
        }

        private void Awake() {
            GameObject laserPointerGameObject = new GameObject(GameObjectName.LaserPointer);
            laserPointerGameObject.transform.SetParent(transform, false);
            //laserPointerGameObject.transform.eulerAngles = 90 * Vector3.up;
            _lineRenderer = laserPointerGameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
            _lineRenderer.startWidth = _laserThickeness;
            _lineRenderer.loop = false;
            _lineRenderer.material = _material;
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        }

    }

}
