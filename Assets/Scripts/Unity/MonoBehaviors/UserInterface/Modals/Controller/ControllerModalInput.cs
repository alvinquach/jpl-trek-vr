using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public class ControllerModalInput : MonoBehaviour, IBrowserUI {

        // TODO Make this a user option
        private const float ScrollSpeedMultiplier = 5.0f;

        private readonly KeyEvents keyEvents = new KeyEvents();

        public bool MouseHasFocus { get; } = false;

        public Vector2 MousePosition { get; } = new Vector2(float.NaN, float.NaN);

        public MouseButton MouseButtons { get; } = 0;

        public Vector2 MouseScroll { get; private set; } = Vector2.zero;

        public bool KeyboardHasFocus { get; private set; }

        public List<Event> KeyEvents {
            get => keyEvents.Events;
        }

        public BrowserCursor BrowserCursor { get; private set; }

        public BrowserInputSettings InputSettings { get; } = new BrowserInputSettings();

        public Browser Browser { get; private set; }

        #region Unity lifecycle functions

        private void Awake() {

            Debug.Log("XRBrowser awake");

            // BrowserCursor cannot be instantiated in constructor,
            // so it has to be done in the Awake() function instead.
            BrowserCursor = new BrowserCursor();

            Browser = GetComponent<Browser>();
            Browser.UIHandler = this;
        }

        #endregion

        public void InputUpdate() {
            keyEvents.InputUpdate();
        }

        public void RegisterKeyDown(KeyCode keyCode) {
            keyEvents.Press(keyCode);
        }

        public void RegisterKeyUp(KeyCode keyCode) {
            keyEvents.Release(keyCode);
        }

        public void RegisterKeyPress(KeyCode keyCode) {
            keyEvents.Press(keyCode);
            StartCoroutine(KeyUp(keyCode));
        }

        public void SetVisiblityState(bool visible) {
            KeyboardHasFocus = visible;
        }

        private IEnumerator KeyUp(KeyCode keyCode) {
            yield return new WaitForSeconds(0.1f);
            keyEvents.Release(keyCode);
        }

    }
}