using System;
using UnityEngine;
using static TrekVRApplication.LocalTerrainConstants;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class LocalTerrainBoundingBoxSelectionController : TerrainBoundingBoxSelectionController {

        private LocalTerrainOverlayController _overlayController;
        protected override TerrainOverlayController OverlayController => _overlayController;

        protected override float IndicatorThickness => CoordinateIndicatorThickness;

        protected override float IndicatorActiveThickness => CoordinateIndicatorActiveThickness;

        protected override void Awake() {
            base.Awake();
            _overlayController = LocalTerrainOverlayController.Instance;
        }

        public override float UpdateCursorPosition(RaycastHit hit) {

            Vector2 coord = GetCoordFromHit(hit);
            LineRenderer lineRenderer = CurrentSelectionIndicator;
            float angle, position;

            // Longitude selection
            if (_selectionIndex % 2 == 0) {
                position = hit.textureCoord.x;
                lineRenderer.SetPosition(0, new Vector2(position, 0));
                lineRenderer.SetPosition(1, new Vector2(position, 1));
                angle = coord.y;
            }

            // Latitude selection
            else {
                position = hit.textureCoord.y;
                lineRenderer.SetPosition(0, new Vector2(0, position));
                lineRenderer.SetPosition(1, new Vector2(1, position));
                angle = coord.x;
            }

            _overlayController.UpdateTexture();

            // Send updated to controller modal.
            if (_framesSinceLastControllerModalUpdate >= ControllerModalUpdateInterval) {
                BoundingBox bbox = new BoundingBox(_selectionBoundingBox);
                bbox[_selectionIndex] = angle;
                SendBoundingBoxUpdateToControllerModal(bbox);
            }

            _framesSinceLastControllerModalUpdate++;
            return angle;
        }

        private LocalTerrainModel GetActiveTerrainModel() {
            TerrainModel terrainModel = TerrainModelManager.Instance.CurrentVisibleModel;
            if (terrainModel is LocalTerrainModel) {
                return (LocalTerrainModel)terrainModel;
            }
            return null;
        }

        protected override Vector2 GetCoordFromHit(RaycastHit hit) {
            LocalTerrainModel terrainModel = GetActiveTerrainModel();
            if (!terrainModel) {
                throw new Exception("Active localized terrain model not found.");
            }
            return BoundingBoxUtils.UVToCoordinates(terrainModel.SquareBoundingBox, hit.textureCoord);
        }

    }

}
