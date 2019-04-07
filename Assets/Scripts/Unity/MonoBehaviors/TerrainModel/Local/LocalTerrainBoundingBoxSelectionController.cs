using System;
using UnityEngine;
using static TrekVRApplication.GlobeTerrainConstants;
using static TrekVRApplication.TerrainOverlayUtils;

namespace TrekVRApplication {

    public class LocalTerrainBoundingBoxSelectionController : TerrainBoundingBoxSelectionController {

        private LocalTerrainOverlayController _overlayController;

        protected override void Awake() {
            base.Awake();
            _overlayController = LocalTerrainOverlayController.Instance;
        }

        public override void MakeBoundarySelection(RaycastHit hit) {

            Vector2 coord = GetCoordFromHit(hit);
            float angle;

            // Longitude selection
            if (_selectionIndex % 2 == 0) {
                angle = coord.y;
                Debug.Log($"Lon selection: {angle} degrees");
            }

            // Latitude selection
            else {
                angle = coord.x;
                Debug.Log($"Lat selection: {angle} degrees");
            }

            _selectionBoundingBox[_selectionIndex] = angle;

            // TODO Calculate line thickness to account for texture projection.
            CurrentSelectionIndicator.startWidth = CoordinateIndicatorThickness;
            _selectionIndex++;


            // Check if selection is finished.
            if (_selectionIndex == 4) {
                Debug.Log("Selection Complete: " + _selectionBoundingBox);
                TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
                TerrainModel terrainModel = terrainModelManager.CreateSubsetLocalModel(
                    terrainModelManager.CurrentVisibleModel,
                    _selectionBoundingBox
                );
                terrainModelManager.ShowTerrainModel(terrainModel, false);
                ExitSelectionMode();
            }
            else {
                ActivateCurrentIndicator();
                _overlayController.UpdateTexture();
            }

            SendBoundingBoxUpdateToControllerModal(new BoundingBox(_selectionBoundingBox));
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
            if (_framesSinceLastControllerModalUpdate >= ControllerModalBoundingBoxUpdateInterval) {
                BoundingBox bbox = new BoundingBox(_selectionBoundingBox);
                bbox[_selectionIndex] = angle;
                SendBoundingBoxUpdateToControllerModal(bbox);
            }

            _framesSinceLastControllerModalUpdate++;
            return angle;
        }

        protected override void ActivateCurrentIndicator() {
            // TODO Calculate line thickness to account for texture projection.
            CurrentSelectionIndicator.startWidth = CoordinateIndicatorActiveThickness;
            CurrentSelectionIndicator.enabled = true;
        }

        protected override void GenerateSelectionIndicatorLines() {

            // Create material for coordinate indicators
            _coordinateIndicatorMaterial = new Material(Shader.Find("Unlit/Color"));
            _coordinateIndicatorMaterial.SetColor("_Color", CoordinateIndicatorColor);

            _lonSelectionStartIndicator = DrawRenderTextureLine(
                Vector2.zero,
                Vector2.up,
                transform,
                _coordinateIndicatorMaterial,
                CoordinateIndicatorThickness,
                $"Lon{GameObjectName.SelectionIndicator}1"
            );

            _latSelectionStartIndicator = DrawRenderTextureLine(
                Vector2.zero,
                Vector2.right,
                transform,
                _coordinateIndicatorMaterial,
                CoordinateIndicatorThickness,
                $"Lat{GameObjectName.SelectionIndicator}1"
            );

            _lonSelectionEndIndicator = DrawRenderTextureLine(
                Vector2.right,
                Vector2.one,
                transform,
                _coordinateIndicatorMaterial,
                CoordinateIndicatorThickness,
                $"Lon{GameObjectName.SelectionIndicator}2"
            );

            _latSelectionEndIndicator = DrawRenderTextureLine(
                Vector2.up,
                Vector2.one,
                transform,
                _coordinateIndicatorMaterial,
                CoordinateIndicatorThickness,
                $"Lat{GameObjectName.SelectionIndicator}2"
            );

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

        private LocalTerrainModel GetActiveTerrainModel() {
            TerrainModel terrainModel = TerrainModelManager.Instance.CurrentVisibleModel;
            if (terrainModel is LocalTerrainModel) {
                return (LocalTerrainModel)terrainModel;
            }
            return null;
        }

        private Vector2 GetCoordFromHit(RaycastHit hit) {
            LocalTerrainModel terrainModel = GetActiveTerrainModel();
            if (!terrainModel) {
                throw new Exception("Active localized terrain model not found.");
            }
            return BoundingBoxUtils.UVToCoordinates(terrainModel.SquareBoundingBox, hit.textureCoord);
        }

    }

}
