using UnityEngine;
using static TrekVRApplication.TerrainModelConstants;
using static TrekVRApplication.GlobeTerrainModelConstants;
using static TrekVRApplication.TerrainOverlayUtils;

namespace TrekVRApplication {

    public class GlobeBoundingBoxSelectionController : TerrainBoundingBoxSelectionController {

        private POILabel _coordSelectionLabel;

        #region Unity lifecycle methods

        protected override void OnEnable() {
            base.OnEnable();
            _coordSelectionLabel.gameObject.SetActive(true);
        }

        #endregion

        public override void MakeBoundarySelection(RaycastHit hit) {

            Vector3 direction = transform.InverseTransformPoint(hit.point);
            Vector3 flattened = new Vector3(direction.x, 0, direction.z);
            float angle;

            // Longitude selection
            if (_selectionIndex % 2 == 0) {
                angle = Vector3.Angle(flattened, -Vector3.forward) - 180;
                if (Vector3.Cross(flattened, Vector3.forward).y > 0) {
                    angle = -angle;
                }
                Debug.Log($"Lon selection: {angle} degrees");
            }

            // Latitude selection
            else {
                angle = Vector3.Angle(direction, flattened);
                if (direction.y < 0) {
                    angle = -angle;
                }
                Debug.Log($"Lat selection: {angle} degrees");
            }

            _selectionBoundingBox[_selectionIndex] = angle;

            CurrentSelectionIndicator.startWidth = CoordinateIndicatorThickness;
            _selectionIndex++;

            // Check if selection is finished.
            if (_selectionIndex == 4) {
                Debug.Log("Selection Complete: " + _selectionBoundingBox);
                TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
                TerrainModel terrainModel = terrainModelManager.CreateLocalModelFromSubset(
                    terrainModelManager.CurrentVisibleModel,
                    _selectionBoundingBox
                );
                terrainModelManager.ShowTerrainModel(terrainModel, false);
                terrainModelManager.GlobeModel.InteractionController.SwitchToActivity(XRInteractableTerrainActivity.Default);
            }
            else {
                ActivateCurrentIndicator();
            }

            SendBoundingBoxUpdateToControllerModal(new BoundingBox(_selectionBoundingBox));
        }

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
            _coordSelectionLabel.transform.position = hit.point;
            _coordSelectionLabel.transform.forward = -hit.normal;

            // Get the current coordinate indicator to set its angles.
            LineRenderer currentCoordinateIndicator = CurrentSelectionIndicator;

            Vector3 direction = transform.InverseTransformPoint(hit.point);
            Vector3 flattened = new Vector3(direction.x, 0, direction.z);

            float angle;

            // Longitude selection
            if (_selectionIndex % 2 == 0) {
                angle = Vector3.Angle(flattened, -Vector3.forward) - 180;
                if (Vector3.Cross(flattened, Vector3.forward).y > 0) {
                    angle = -angle;
                }
                currentCoordinateIndicator.transform.localEulerAngles = new Vector3(0, -angle, 0);

                _coordSelectionLabel.Text = $"Lon: {angle.ToString("0.00")}°";
            }

            // Latitude selection
            else {
                angle = Vector3.Angle(direction, flattened);
                if (direction.y < 0) {
                    angle = -angle;
                }
                Vector2 offsetAndScale = CalculateLatitudeIndicatorOffsetAndScale(angle);
                float modelRadius = Mars.Radius * TerrainModelScale;

                currentCoordinateIndicator.transform.localPosition = new Vector3(0, modelRadius * offsetAndScale.y, 0);
                currentCoordinateIndicator.transform.localScale = (offsetAndScale.x * modelRadius + CoordinateIndicatorRadiusOffset) * Vector3.one;

                _coordSelectionLabel.Text = $"Lat: {angle.ToString("0.00")}°";
            }

            // Send updated to controller modal.
            if (_framesSinceLastControllerModalUpdate >= ControllerModalBoundingBoxUpdateInterval) {
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

        protected override void ActivateCurrentIndicator() {
            CurrentSelectionIndicator.startWidth = CoordinateIndicatorActiveThickness;
            CurrentSelectionIndicator.enabled = true;
        }

        protected override void GenerateSelectionIndicatorLines() {

            float indicatorRadius = Mars.Radius * TerrainModelScale + CoordinateIndicatorRadiusOffset;

            // Create material for coordinate indicators
            _coordinateIndicatorMaterial = new Material(Shader.Find("Unlit/Color"));
            _coordinateIndicatorMaterial.SetColor("_Color", CoordinateIndicatorColor);

            GameObject lonSelectionStartIndicator = new GameObject($"Lon{GameObjectName.SelectionIndicator}1") {
                layer = (int)CullingLayer.Terrain // TODO Make a new layer for coordinate lines and labels
            };
            lonSelectionStartIndicator.transform.SetParent(transform, false);
            lonSelectionStartIndicator.transform.localScale = indicatorRadius * Vector3.one;
            _lonSelectionStartIndicator = InitCoordinateIndicator(
                lonSelectionStartIndicator,
                _coordinateIndicatorMaterial,
                CoordinateIndicatorThickness
            );
            _lonSelectionStartIndicator.enabled = false;
            GeneratePointsForLongitudeIndicator(_lonSelectionStartIndicator);

            GameObject latSelectionStartIndicator = new GameObject($"Lat{GameObjectName.SelectionIndicator}1") {
                layer = (int)CullingLayer.Terrain // TODO Make a new layer for coordinate lines and labels
            };
            latSelectionStartIndicator.transform.SetParent(transform, false);
            _latSelectionStartIndicator = InitCoordinateIndicator(
                latSelectionStartIndicator,
                _coordinateIndicatorMaterial,
                CoordinateIndicatorThickness
            );
            _latSelectionStartIndicator.enabled = false;
            GeneratePointsForLatitudeIndicator(_latSelectionStartIndicator);

            GameObject lonSelectionEndIndicator = new GameObject($"Lon{GameObjectName.SelectionIndicator}2") {
                layer = (int)CullingLayer.Terrain // TODO Make a new layer for coordinate lines and labels
            };
            lonSelectionEndIndicator.transform.SetParent(transform, false);
            lonSelectionEndIndicator.transform.localScale = indicatorRadius * Vector3.one;
            _lonSelectionEndIndicator = InitCoordinateIndicator(
                lonSelectionEndIndicator,
                _coordinateIndicatorMaterial,
                CoordinateIndicatorThickness
            );
            _lonSelectionEndIndicator.enabled = false;
            GeneratePointsForLongitudeIndicator(_lonSelectionEndIndicator);

            GameObject latSelectionEndIndicator = new GameObject($"Lat{GameObjectName.SelectionIndicator}2") {
                layer = (int)CullingLayer.Terrain // TODO Make a new layer for coordinate lines and labels
            };
            latSelectionEndIndicator.transform.SetParent(transform, false);
            _latSelectionEndIndicator = InitCoordinateIndicator(
                latSelectionEndIndicator,
                _coordinateIndicatorMaterial,
                CoordinateIndicatorThickness
            );
            _latSelectionEndIndicator.enabled = false;
            GeneratePointsForLatitudeIndicator(_latSelectionEndIndicator);

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
            if (disable) {
                _lonSelectionStartIndicator.enabled = false;
                _latSelectionStartIndicator.enabled = false;
                _lonSelectionEndIndicator.enabled = false;
                _latSelectionEndIndicator.enabled = false;
            }
        }

    }

}
