using UnityEngine;

namespace TrekVRApplication {

    public class XRInteractableTerrainSection : XRInteractableTerrain {

        private SectionTerrainModel _terrainModel;
        public override TerrainModel TerrainModel => _terrainModel;

        #region Unity lifecycle methods
        void Start() {
            _terrainModel = GetComponent<SectionTerrainModel>();
        }

        #endregion

        #region Event handlers

        public override void OnTriggerDown(XRController sender, RaycastHit hit, ClickedEventArgs e) {
            BoundingBox bbox = _terrainModel.SquareBoundingBox;
            Vector2 coord = BoundingBoxUtils.UVToCoordinates(bbox, hit.textureCoord);
            _terrainModel.EnableOverlay = !_terrainModel.EnableOverlay;
            Debug.Log($"Clicked terrain at ({coord.x}°, {coord.y}°).");
        }

        #endregion

    }

}