using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    /// <summary>
    ///     Implementation of ZFBrowser's IBrowserUI interface used to provide
    ///     user input to the embedded browser.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Browser))]
    public class XRBrowser : XRInteractableObject, IBrowserUI {

        private Browser _browser;

        private MeshRenderer _meshRenderer;

        private MeshCollider _meshCollider;

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

        private bool _visible;
        public bool Visible {
            get { return _visible; }
            set { SetVisiblity(value); }
        }

        public bool MouseHasFocus { get; private set; }

        public Vector2 MousePosition { get; private set; } = new Vector2(float.NaN, float.NaN);

        public MouseButton MouseButtons { get; private set; }

        public Vector2 MouseScroll { get; private set; } = Vector2.zero;

        public bool KeyboardHasFocus { get; private set; }

        public List<Event> KeyEvents { get; private set; }

        public BrowserCursor BrowserCursor { get; private set; }

        public BrowserInputSettings InputSettings { get; private set; } = new BrowserInputSettings();

        #region Unity lifecycle functions

        private void Awake() {

            // BrowserCursor cannot be instantiated in constructor,
            // so it has to be done in the Awake() function instead.
            BrowserCursor = new BrowserCursor();

            _browser = GetComponent<Browser>();
            _browser.UIHandler = this;
            _browser.onLoad += loadData => {
                ZFBrowserUtils.RegisterStandardFunctions(_browser);
            };

            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            if (hideAfterInit) {
                SetVisiblity(false);
            }

        }

        #endregion

        #region Event handlers

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            MouseButtons = MouseButton.Left;
        }

        public override void OnTriggerUp(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            MouseButtons = 0;
        }

        public override void OnCursorOver(XRController sender, RaycastHit hit) {
            MousePosition = hit.textureCoord; 
        }

        public override void OnCursorEnter(XRController sender, RaycastHit hit) {
            _browser.EnableInput = true;
            MouseHasFocus = true;
        }

        public override void OnCursorLeave(XRController sender, RaycastHit hit) {
            _browser.EnableInput = false;
            MouseButtons = 0;
            MousePosition = new Vector3(float.NaN, float.NaN);
            MouseHasFocus = false;
        }

        #endregion

        public void InputUpdate() {
            // TODO Implement this
        }

        private void SetVisiblity(bool visible) {
            _visible = visible;
            _browser.EnableInput = visible;
            _browser.EnableRendering = visible;

            // If visiblilty was set to false then hide the mesh renderer
            // and mesh collider immediately.
            if (!_visible) {
                MouseButtons = 0;
                MousePosition = new Vector3(float.NaN, float.NaN);
                MouseHasFocus = false;
                if (_meshRenderer) {
                    _meshRenderer.enabled = false;
                }
                if (_meshCollider) {
                    _meshCollider.enabled = false;
                }
            }

            // If visiblilty was set to true, the mesh renderer and mesh 
            // collider need to be unhidden, but is delayed to give the
            // browser a chance re-render the contents first.
            else {
                // TODO Add variables to set the behavior of the browser
                // after unhiding (ie. whether to go back to root menu
                // or keep displaying same page).
                StartCoroutine(OnUnhide());
            }
        }

        private IEnumerator OnUnhide() {
            yield return new WaitForSeconds(0.1f); // TODO Fix magic number.
            MouseHasFocus = true;
            if (_meshRenderer) {
                _meshRenderer.enabled = true;
            }
            if (_meshCollider) {
                _meshCollider.enabled = true;
            }
        }

    }

}
