using System;
using UnityEngine;
using static TrekVRApplication.GlobeTerrainConstants;
using static TrekVRApplication.ZFBrowserConstants;

namespace TrekVRApplication {

    public class GlobeTerrainBoundingBoxSelectionController : TerrainBoundingBoxSelectionController {

        private GlobeTerrainOverlayController _overlayController;
        protected override TerrainOverlayController OverlayController => _overlayController;

        protected override float IndicatorThickness => CoordinateIndicatorThickness;

        protected override float IndicatorActiveThickness => CoordinateIndicatorActiveThickness;

        private POILabel _coordSelectionLabel;

        #region Unity lifecycle methods

        protected override void Awake() {
            base.Awake();
            _overlayController = GlobeTerrainOverlayController.Instance;
            _coordSelectionLabel.gameObject.SetActive(false);
        }

        #endregion

        /// <summary>
        ///     <para>
        ///         Under select mode, there are currently two things that happen 
        ///         when the cursor is pointed somewhere within the planet model:
        ///     </para>
        ///     <para>
        ///         1. A line indicating where current longitude or latitude is, 
        ///         referred to as a coordinate indicator, moves along with the
        ///         cursor position.
        ///     </para>
        ///     <para>
        ///         2. A text overlay at the cursor location indicatest the  
        ///         current longitude or latitude angle.
        ///     </para>
        ///     </summary>
        public override float UpdateCursorPosition(RaycastHit hit) {

            // Update the position and angle of the coordinate selection label.
            _coordSelectionLabel.gameObject.SetActive(true); // TODO move this so it doesn't get called every update.
            _coordSelectionLabel.transform.position = hit.point;
            _coordSelectionLabel.transform.forward = -hit.normal;
            
            Vector2 coord = GetCoordFromHit(hit);
            LineRenderer lineRenderer = CurrentSelectionIndicator;
            float angle, position;

            // Longitude selection
            if (_selectionIndex % 2 == 0) {
                position = 2 * hit.textureCoord.x;
                lineRenderer.SetPosition(0, new Vector2(position, 0));
                lineRenderer.SetPosition(1, new Vector2(position, 1));
                angle = coord.y;

                _coordSelectionLabel.Text = $"Lon: {angle.ToString("0.00")}°";
            }

            // Latitude selection
            else {
                position = hit.textureCoord.y;
                lineRenderer.SetPosition(0, new Vector2(0, position));
                lineRenderer.SetPosition(1, new Vector2(2, position));
                angle = coord.x;

                _coordSelectionLabel.Text = $"Lat: {angle.ToString("0.00")}°";
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

        protected override void ExitSelectionMode() {
            base.ExitSelectionMode();
            _coordSelectionLabel.gameObject.SetActive(false);
        }

        protected override void GenerateSelectionIndicatorLines() {
            base.GenerateSelectionIndicatorLines();

            // Instantiate a copy of the coordinate template to display the coordiate values.
            GameObject coordinateTemplate = TemplateService.Instance.GetTemplate(GameObjectName.CoordinateTemplate);
            coordinateTemplate.layer = (int)CullingLayer.Terrain; // TODO Make a new layer for coordinate lines and labels
            if (coordinateTemplate) {
                GameObject copy = Instantiate(coordinateTemplate);
                copy.transform.SetParent(transform); // TODO Move this to a container for labels.
                _coordSelectionLabel = copy.transform.GetComponent<POILabel>();
            }

        }
        protected override void ResetIndicatorPositions(bool disable) {
            for (int i = 0; i < 4; i++) {
                LineRenderer lineRenderer = GetSelectionIndicatorByIndex(i);
                if (i % 2 == 0) {
                    lineRenderer.SetPosition(0, new Vector2(0, 0));
                    lineRenderer.SetPosition(1, new Vector2(0, 1));
                }
                else {
                    lineRenderer.SetPosition(0, new Vector2(0, 0));
                    lineRenderer.SetPosition(1, new Vector2(1, 0));
                }
                if (disable) {
                    lineRenderer.enabled = false;
                }
            }
            _overlayController.UpdateTexture();
        }

        private GlobeTerrainModel GetActiveTerrainModel() {
            TerrainModel terrainModel = TerrainModelManager.Instance.CurrentVisibleModel;
            if (terrainModel is GlobeTerrainModel) {
                return (GlobeTerrainModel)terrainModel;
            }
            return null;
        }

        protected override Vector2 GetCoordFromHit(RaycastHit hit) {
            GlobeTerrainModel terrainModel = GetActiveTerrainModel();
            if (!terrainModel) {
                throw new Exception("Globe model is not currently active.");
            }
            return BoundingBoxUtils.UVToCoordinates(UnrestrictedBoundingBox.Global, hit.textureCoord);
        }

    }

}
