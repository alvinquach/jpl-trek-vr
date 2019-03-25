using UnityEngine;
using System;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class TerrainModelOverlayController : MonoBehaviour {

        // TODO Move this somewhere else
        private const int RenderTextureSize = 2048;

        public static TerrainModelOverlayController Instance;

        [SerializeField]
        private RenderTexture _renderTexture;
        public RenderTexture RenderTexture {
            get => _renderTexture;
        }

        [SerializeField]
        private Camera _renderTextureCamera;
        public Camera RenderTextureCamera {
            get => _renderTextureCamera;
        }

        private void Awake() {

            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                throw new Exception("Only one instace of TerrainModelOverlayController is allowed!");
            }

            _renderTexture = new RenderTexture(RenderTextureSize, RenderTextureSize, 0);
            _renderTexture.format = RenderTextureFormat.ARGB32;
            _renderTexture.Create();

            _renderTextureCamera.aspect = 1f;
            _renderTextureCamera.targetTexture = RenderTexture;
            _renderTextureCamera.enabled = false; // Disable automatic updates
            _renderTextureCamera.Render();
        }

    }

}