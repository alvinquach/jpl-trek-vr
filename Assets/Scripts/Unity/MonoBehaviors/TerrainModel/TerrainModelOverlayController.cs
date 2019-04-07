using UnityEngine;

namespace TrekVRApplication {

    public abstract class TerrainModelOverlayController : MonoBehaviour {

        public const float CameraVerticalSize = 0.5f;

        /// <summary>
        ///     Vertical resolution of the render texture.
        /// </summary>
        [SerializeField]
        [Tooltip("Vertical resolution of the render texture.")]
        protected int _renderTextureResolution = 2048;

        /// <summary>
        ///     Aspect ratio of the render texture and camera.
        /// </summary>
        [SerializeField]
        [Tooltip("Aspect ratio of the render texture and camera.")]
        protected float _renderTextureAspectRatio = 1.0f;

        protected GameObject _renderTextureObjectsContainer;

        public RenderTexture RenderTexture { get; private set; }

        public Camera RenderTextureCamera { get; private set; }

        public LocalTerrainBoundingBoxSelectionController BBoxSelectionController { get; private set; }

        protected virtual void Awake() {

            // Create the render texture.
            int horizontalResolution = Mathf.RoundToInt(_renderTextureAspectRatio * _renderTextureResolution);
            RenderTexture = new RenderTexture(horizontalResolution, _renderTextureResolution, 0) {
                format = RenderTextureFormat.ARGB32
            };
            RenderTexture.Create();

            RenderTextureCamera = GetComponent<Camera>();
            RenderTextureCamera.aspect = _renderTextureAspectRatio;
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
                new Vector3(-CameraVerticalSize * _renderTextureAspectRatio, -CameraVerticalSize, 1);

            // Create the latitude and longitude selection indicators and controller.
            GameObject selectionIndicatorsContainer = new GameObject(GameObjectName.SelectionIndicatorContainer) {
                layer = (int)CullingLayer.RenderToTexture
            };
            selectionIndicatorsContainer.transform.SetParent(_renderTextureObjectsContainer.transform, false);
            BBoxSelectionController = selectionIndicatorsContainer.AddComponent<LocalTerrainBoundingBoxSelectionController>();
            BBoxSelectionController.SetEnabled(false);
        }

        public void UpdateTexture() {
            RenderTextureCamera.Render();
        }

    }

}