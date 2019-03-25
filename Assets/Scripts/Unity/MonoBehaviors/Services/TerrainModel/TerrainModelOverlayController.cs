using UnityEngine;
using System;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class TerrainModelOverlayController : MonoBehaviour {

        // TODO Move this somewhere else
        private const int RenderTextureSize = 2048;

        public static TerrainModelOverlayController Instance;

        public RenderTexture RenderTexture { get; private set; }

        public Camera RenderTextureCamera { get; private set;  }

        [SerializeField]
        private GameObject _renderTextureObjectsContainer;

        public SectionTerrainBoundingBoxSelectionController BBoxSelectionController { get; private set; }

        private void Awake() {

            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                throw new Exception("Only one instace of TerrainModelOverlayController is allowed!");
            }

            RenderTexture = new RenderTexture(RenderTextureSize, RenderTextureSize, 0);
            RenderTexture.format = RenderTextureFormat.ARGB32;
            RenderTexture.Create();

            RenderTextureCamera = GetComponent<Camera>();
            RenderTextureCamera.aspect = 1f;
            RenderTextureCamera.targetTexture = RenderTexture;
            RenderTextureCamera.enabled = false; // Disable automatic updates
            RenderTextureCamera.Render();

            // Create the latitude and longitude selection indicators and controller.
            GameObject selectionIndicatorsContainer = new GameObject(GameObjectName.SelectionIndicatorContainer) {
                layer = (int)CullingLayer.RenderToTexture
            };
            selectionIndicatorsContainer.transform.SetParent(_renderTextureObjectsContainer.transform, false);
            BBoxSelectionController = selectionIndicatorsContainer.AddComponent<SectionTerrainBoundingBoxSelectionController>();
            BBoxSelectionController.SetEnabled(false);

        }

        public void UpdateTexture() {
            RenderTextureCamera.Render();
        }

    }

}