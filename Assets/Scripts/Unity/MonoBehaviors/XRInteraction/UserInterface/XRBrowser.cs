using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    /// <summary>
    ///     Implementation of ZFBrowser's IBrowserUI interface used to provide
    ///     user input to the embedded browser.
    /// </summary>
    public class XRBrowser : XRInteractableObject, IBrowserUI {

        private Browser _browser;

        /// <summary>
        ///     Whether to set the browser game object to inactive after
        ///     Awake() is called. Browser game objects should start out
        ///     as active in order to get browser content load.
        /// </summary>
        [Tooltip(
            "Browser game object should start out as active in order to let" + 
            "browser content load. Check this box to immediately hide the" + 
            "browser after Awake() is called."
        )]
        public bool hideAfterInit;

        public bool MouseHasFocus { get; private set; } = false;

        public Vector2 MousePosition { get; private set; } = new Vector2(float.NaN, float.NaN);

        public MouseButton MouseButtons { get; private set; }

        public Vector2 MouseScroll { get; private set; } = Vector2.zero;

        public bool KeyboardHasFocus { get; private set; }

        public List<Event> KeyEvents { get; private set; }

        public BrowserCursor BrowserCursor { get; private set; }

        public BrowserInputSettings InputSettings { get; private set; } = new BrowserInputSettings();

        private void Awake() {

            // BrowserCursor cannot be instantiated in constructor,
            // so it has to be done in the Awake() function instead.
            BrowserCursor = new BrowserCursor();

            _browser = GetComponent<Browser>();

            if (_browser) {
                _browser.UIHandler = this;
            }

            if (hideAfterInit) {
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer) {
                    meshRenderer.enabled = false;
                }

                MeshCollider meshCollider = GetComponent<MeshCollider>();
                if (meshCollider) {
                    meshCollider.enabled = false;
                }

                _browser.WhenReady(() => {
                    gameObject.SetActive(false);
                    meshRenderer.enabled = meshCollider.enabled = true;
                });
            }

        }

        #region Event handlers

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            Debug.Log("MOUSE DOWN");
            MouseButtons = MouseButton.Left;
        }

        public override void OnTriggerUp(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            Debug.Log("MOUSE UP");
            MouseButtons = 0;
        }

        public override void OnCursorOver(XRController sender, RaycastHit hit) {
            //VectorUtils.Print(new Vector3(hit.textureCoord.x * _browser.Size.x, hit.textureCoord.y * _browser.Size.y, 0));
            MousePosition = hit.textureCoord;
            //MousePosition = new Vector2(hit.textureCoord.x, 1 - hit.textureCoord.y);
        }

        public override void OnCursorEnter(XRController sender, RaycastHit hit) {
            MouseHasFocus = true;
        }

        public override void OnCursorLeave(XRController sender, RaycastHit hit) {
            MouseButtons = 0;
            MousePosition = new Vector3(float.NaN, float.NaN);
            MouseHasFocus = false;
        }

        #endregion

        public void InputUpdate() {
            // TODO Implement this
        }

    }

}
