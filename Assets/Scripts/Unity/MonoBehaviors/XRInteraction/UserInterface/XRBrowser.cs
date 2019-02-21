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

        // TODO Make this a user option
        private const float ScrollSpeedMultiplier = 13.37f;

        private ScrollTransformer _scrollTransformer;

        private Vector2 _previousPadPosition = new Vector2(float.NaN, float.NaN);

        private float _nextXScroll;

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
        public bool hideAfterInit = true;

        private bool _visible;
        public bool Visible {
            get { return _visible; }
            set { SetVisiblity(value); }
        }

        public Browser Browser { get; private set; }

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

            Debug.Log("XRBrowser awake");

            // BrowserCursor cannot be instantiated in constructor,
            // so it has to be done in the Awake() function instead.
            BrowserCursor = new BrowserCursor();

            Browser = GetComponent<Browser>();
            Browser.UIHandler = this;
            Browser.onLoad += loadData => {
                ZFBrowserUtils.RegisterStandardFunctions(Browser);
            };

            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            if (hideAfterInit) {
                SetVisiblity(false);
            }

            _scrollTransformer = new ScrollTransformer();
            _scrollTransformer.enableX = false; // Disable scrolling in the x-direction.
            _scrollTransformer.OutputBounds *= ScrollSpeedMultiplier;

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
            Browser.EnableInput = true;
            MouseHasFocus = true;
        }

        public override void OnCursorLeave(XRController sender, RaycastHit hit) {
            Browser.EnableInput = false;
            MouseButtons = 0;
            MousePosition = new Vector3(float.NaN, float.NaN);
            MouseHasFocus = false;
        }

        public override void OnPadUntouch(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            _previousPadPosition = new Vector2(float.NaN, float.NaN);
        }

        public override void OnPadSwipe(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            Vector2 padPosition = new Vector2(-e.padX, e.padY);
            if (!float.IsNaN(_previousPadPosition.x) && !float.IsNaN(_previousPadPosition.y)) {
                Vector2 delta = padPosition - _previousPadPosition;
                _scrollTransformer.AddInputSample(delta);
            }
            _previousPadPosition = padPosition;
        }


        #endregion

        public void InputUpdate() {

            // If there was a previous scroll command for the x-direction,
            // then complete it in this update.
            if (!float.IsNaN(_nextXScroll)) {
                MouseScroll = new Vector2(_nextXScroll, 0);
                _nextXScroll = float.NaN;
            }

            // Check is new scroll command is ready for this update.
            else if (_scrollTransformer.OutputSampleReady) {
                Vector2 scroll = _scrollTransformer.GetOutputSample();

                // If the scroll in the y-direction is non-zero, then do it first and
                // save the x-direction for the next update if it is also non-zero.
                if (!MathUtils.CompareFloats(scroll.y, 0)) {
                    MouseScroll = new Vector2(0, scroll.y);

                    if (!MathUtils.CompareFloats(scroll.x, 0)) {
                        _nextXScroll = scroll.x;
                    }
                }

                // If y-direction is zero, then do x-direction instead, even if it's also zero.
                else {
                    MouseScroll = new Vector2(scroll.x, 0);
                }
            }

            // If there was no previous x-direction scroll command, and there
            // is no new scroll command, then reset.
            else {
                _nextXScroll = float.NaN;
                MouseScroll = Vector2.zero;
            }

        }

        private void SetVisiblity(bool visible) {
            _visible = visible;
            Browser.EnableInput = visible;
            Browser.EnableRendering = visible;

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
