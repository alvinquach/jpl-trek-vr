using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    public abstract class TerrainOverlayController : MonoBehaviour {

        public const float CameraVerticalSize = 0.5f;

        /// <summary>
        ///     Vertical resolution of the render texture.
        /// </summary>
        [SerializeField]
        [Tooltip("Vertical resolution of the render texture. Do not change this after application has started.")]
        protected int _renderTextureResolution = 2048;

        /// <summary>
        ///     Aspect ratio of the render texture and camera.
        /// </summary>
        public abstract float RenderTextureAspectRatio { get; }

        public RenderTexture RenderTexture { get; private set; }

        public Camera RenderTextureCamera { get; private set; }

        /// <summary>
        ///     Whether the camera will have to be re-renedered during the update. 
        /// </summary>
        protected bool _textureUpdateRequired = false;

        public LocalTerrainBoundingBoxSelectionController BBoxSelectionController { get; private set; }

        protected GameObject _renderTextureObjectsContainer;

        protected readonly ISet<TerrainOverlayObject> _overlayObjects = new HashSet<TerrainOverlayObject>();

        public abstract IBoundingBox CurrentBoundingBox { get; }

        #region Unity lifecycle functions

        protected virtual void Awake() {

            // Create the render texture.
            int horizontalResolution = Mathf.RoundToInt(RenderTextureAspectRatio * _renderTextureResolution);
            RenderTexture = new RenderTexture(horizontalResolution, _renderTextureResolution, 0) {
                format = RenderTextureFormat.ARGB32
            };
            RenderTexture.Create();

            // Configure the render texture camera.
            RenderTextureCamera = GetComponent<Camera>();
            RenderTextureCamera.aspect = RenderTextureAspectRatio;
            RenderTextureCamera.orthographicSize = CameraVerticalSize;
            RenderTextureCamera.targetTexture = RenderTexture;
            RenderTextureCamera.enabled = false; // Disable automatic updates
            RenderTextureCamera.Render();

            // Create and orient the objects container.
            _renderTextureObjectsContainer = new GameObject("Objects") {
                layer = (int)CullingLayer.RenderToTexture,
            };
            _renderTextureObjectsContainer.transform.SetParent(transform, false);
            _renderTextureObjectsContainer.transform.localPosition = 
                new Vector3(-CameraVerticalSize * RenderTextureAspectRatio, -CameraVerticalSize, 1);

            // Create the latitude and longitude selection indicators and controller.
            GameObject selectionIndicatorsContainer = new GameObject(GameObjectName.SelectionIndicatorContainer) {
                layer = (int)CullingLayer.RenderToTexture
            };
            selectionIndicatorsContainer.transform.SetParent(_renderTextureObjectsContainer.transform, false);
            BBoxSelectionController = selectionIndicatorsContainer.AddComponent<LocalTerrainBoundingBoxSelectionController>();
            BBoxSelectionController.SetEnabled(false);
        }

        private void LateUpdate() {
            if (_textureUpdateRequired) {
                RenderTextureCamera.Render();
                _textureUpdateRequired = false;
            }
        }

        #endregion

        public void UpdateTexture(bool force = false) {
            if (force) {
                RenderTextureCamera.Render();
            }
            _textureUpdateRequired = !force;
        }

        /// <summary>
        ///     Removes all objects from the overlay. All removed objects will
        ///     have their gameObjects also destroyed (which will also destroy
        ///     the object itself).
        /// </summary>
        public void ClearObjects() {
            foreach (TerrainOverlayObject overlayObject in _overlayObjects) {
                Destroy(overlayObject.gameObject);
            }
            _overlayObjects.Clear();
        }

        /// <summary>
        ///     Removes an object from the overlay if it exists in this overlay.
        ///     The removed object will have its gameObject destroyed, which
        ///     will also destroy the object itself.
        /// </summary>
        public void RemoveObject(TerrainOverlayObject overlayObject, bool updateTexture = true) {
            if (_overlayObjects.Remove(overlayObject)) {
                Destroy(overlayObject.gameObject);
                if (updateTexture) {
                    UpdateTexture();
                }
            }
        }


        #region Add area methods

        public TerrainOverlayArea AddArea(Color32 color, string name = null) {
            Material material = new Material(Shader.Find("Custom/Unlit/TransparentColor"));
            material.SetColor("_Color", color);
            return AddArea(material, name);
        }

        public TerrainOverlayArea AddArea(Material material, string name = null) {
            TerrainOverlayArea overlayArea = AddObject<TerrainOverlayArea>(name);
            overlayArea.Material = material;
            _overlayObjects.Add(overlayArea);
            return overlayArea;
        }

        #endregion

        #region Add line methods

        public TerrainOverlayLine AddLine(Color32 color, string name = null) {
            Material material = new Material(Shader.Find("Custom/Unlit/TransparentColor"));
            material.SetColor("_Color", color);
            return AddLine(material, name);
        }

        public TerrainOverlayLine AddLine(Material material, string name = null) {
            TerrainOverlayLine overlayLine = AddObject<TerrainOverlayLine>(name);
            overlayLine.Material = material;
            _overlayObjects.Add(overlayLine);
            return overlayLine;
        }

        #endregion

        private T AddObject<T>(string name) where T : TerrainOverlayObject {
            if (string.IsNullOrEmpty(name)) {
                name = typeof(T).Name;
            }
            GameObject gameObject = new GameObject(name) {
                layer = (int)CullingLayer.RenderToTexture
            };
            gameObject.transform.SetParent(_renderTextureObjectsContainer.transform, false);
            return gameObject.AddComponent<T>();
        }

    }

}