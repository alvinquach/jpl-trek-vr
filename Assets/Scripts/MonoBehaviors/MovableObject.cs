using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour {

    [SerializeField] private float _angularDeceleration = 0.1f;

    [SerializeField] private float _linearDeceleration = 0.1f;

    [SerializeField] private int _decelerationSmoothing = 10;

    private List<Vector3> _positionHistory;

    private List<Vector3> _angleHistory;

    private Vector3 _linearVelocity = Vector3.zero;

    //

    private bool _grabbed = false;

    public bool grabbed {
        get {
            return _grabbed;
        }
        set {
            _grabbed = value;
            if (!_grabbed) {
                GrabEnd();
            }
        }
    }

    private void Start() {
        _positionHistory = new List<Vector3>(_decelerationSmoothing);
        _angleHistory = new List<Vector3>(_decelerationSmoothing);
    }

    // Update is called once per frame
    void Update() {
        if (_grabbed) {

            if (_positionHistory.Count == _positionHistory.Capacity) {
                _positionHistory.RemoveAt(0);
            }
            _positionHistory.Add(transform.position);

            if (_angleHistory.Count == _angleHistory.Capacity) {
                _angleHistory.RemoveAt(0);
            }
            _angleHistory.Add(transform.position);
        }

        else {

            float _linearSpeed = _linearVelocity.magnitude;
            if (_linearSpeed > 0) {
                Debug.Log(_linearSpeed);
                transform.position += _linearVelocity;
                _linearVelocity = Mathf.Clamp(_linearSpeed - _linearDeceleration, 0, _linearSpeed) * _linearVelocity.normalized;
            }

        }
    }

    private void GrabEnd() {
        _linearVelocity = (_positionHistory[_positionHistory.Count - 1] - _positionHistory[0]) / _positionHistory.Count;
        Debug.Log(_linearVelocity);
        _positionHistory.Clear();
        _angleHistory.Clear();
    }

}