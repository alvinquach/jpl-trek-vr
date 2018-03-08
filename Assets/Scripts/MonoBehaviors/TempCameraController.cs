using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCameraController : MonoBehaviour {

    private GameObject _camera;

    private bool _cursorLock = false;

    [SerializeField] private float _speed = 0.1f;

    [SerializeField] private float _sprintSpeed = 1.0f;

    [SerializeField] private float _mouseSensitivity = 2.0f;

    // Use this for initialization
    void Start() {

        _camera = transform.Find("Camera").gameObject;

        ToggleCursorLock();


    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown("z")) {
            ToggleCursorLock();
        }

        if (!_cursorLock) {
            return;
        }

        transform.localEulerAngles += _mouseSensitivity * Input.GetAxis("Mouse X") * Vector3.up;

        _camera.transform.localEulerAngles -= _mouseSensitivity * Input.GetAxis("Mouse Y") * Vector3.right;

        float speed = Input.GetKey(KeyCode.LeftShift) ? _sprintSpeed : _speed;

        if (Input.GetKey("w")) {
            transform.position += speed * _camera.transform.forward.normalized;
        }
        if (Input.GetKey("s")) {
            transform.position -= speed * _camera.transform.forward.normalized;
        }
        if (Input.GetKey("a")) {
            transform.position -= speed * transform.right.normalized;
        }
        if (Input.GetKey("d")) {
            transform.position += speed * transform.right.normalized;
        }
        if (Input.GetKey(KeyCode.Space)) {
            transform.position += speed * Vector3.up;
        }
        if (Input.GetKey(KeyCode.LeftControl)) {
            transform.position -= speed * Vector3.up;
        }

    }

    private void ToggleCursorLock() {
        _cursorLock = !_cursorLock;
        Cursor.lockState = _cursorLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !_cursorLock;
    }
}
