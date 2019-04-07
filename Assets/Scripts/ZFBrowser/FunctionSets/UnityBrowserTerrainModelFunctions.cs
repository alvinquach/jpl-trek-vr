using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public class UnityBrowserTerrainModelFunctions : UnityBrowserFunctionSet {

        protected override string FunctionsReadyVariable { get; } = "terrainFunctionsReady";

        public UnityBrowserTerrainModelFunctions(Browser browser) : base(browser) {

        }

        [RegisterToBrowser]
        public void ShowGlobeModel() {
            TerrainModelManager.Instance.ShowGlobeModel();
        }

        [RegisterToBrowser]
        public void NavigateToCoordinate(string bbox) {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            XRInteractableGlobeTerrain globe = (XRInteractableGlobeTerrain)terrainModelManager.GlobeModel.InteractionController;
            if (globe) {
                BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(bbox);
                Vector2 latLon = BoundingBoxUtils.MedianLatLon(boundingBox);
                Camera eye = UserInterfaceManager.Instance.XRCamera;
                globe.NavigateTo(latLon, eye.transform.position);
            }
        }

        [RegisterToBrowser]
        public void ViewLocalTerrainBookmark(string bookmarkJson) {
            Bookmark bookmark = JsonConvert.DeserializeObject<Bookmark>(bookmarkJson, JsonConfig.SerializerSettings);
            BoundingBox bbox = BoundingBoxUtils.ParseBoundingBox(bookmark.BoundingBox);
            TerrainModelProductMetadata baseProductMetadata = new TerrainModelProductMetadata(bookmark.TexturesUUID[0], bbox, 0);
            // TODO Add additional product layers if present.

            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            LocalTerrainModel terrainModel = 
                terrainModelManager.CreateLocalModel(bbox, bookmark.DemUUID, baseProductMetadata, false, false);
            terrainModelManager.ShowTerrainModel(terrainModel);
        }

        [RegisterToBrowser]
        public void GetCurrentViewSettings(string requestId) {
            // TODO Un-hardcode this data
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            XRInteractableGlobeTerrain globe = (XRInteractableGlobeTerrain)terrainModelManager.GlobeModel.InteractionController;
            TerrainModel currentTerrainModel = terrainModelManager.CurrentVisibleModel;
            IDictionary<string, object> settings = new Dictionary<string, object>() {
                { "terrainType", currentTerrainModel is GlobeTerrainModel ? "globe" : "local" },
                { "heightExaggeration", terrainModelManager.HeightExagerration },
                { "textures", terrainModelManager.TerrainTexturesEnabled },
                { "coordinates", globe.EnableCoordinateLines },
                { "locationNames", true },
            };
            ZFBrowserUtils.SendDataResponse(_browser, requestId, settings);
        }

        [RegisterToBrowser]
        public void SetHeightExaggeration(double value) {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            terrainModelManager.HeightExagerration = (float)value;
        }

        [RegisterToBrowser]
        public void SetTexturesVisiblity(bool visible) {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            terrainModelManager.TerrainTexturesEnabled = visible;
        }

        [RegisterToBrowser]
        public void SetCoordinateIndicatorsVisibility(bool visible) {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            if (terrainModelManager.GlobeModelIsVisible()) {
                XRInteractableGlobeTerrain globe = (XRInteractableGlobeTerrain)terrainModelManager.GlobeModel.InteractionController;
                globe.EnableCoordinateLines = visible;
            }
        }

        [RegisterToBrowser]
        public void SetLocationNamesVisibility(bool visible) {
            Debug.Log($"Terrain location names visiblity set to {visible}.");
        }

        // TODO Move this somewhere else?
        [RegisterToBrowser]
        public void HideControlPanel() {
            Scenes.MainRoom.MainRoomTerrainControlPanelGroup controlPanelGroup =
                 Scenes.MainRoom.MainRoomTerrainControlPanelGroup.Instance;
            if (!controlPanelGroup) {
                return;
            }
            controlPanelGroup.ActiveControlPanel.MoveDown();
        }

        // Temporary
        [RegisterToBrowser]
        public void AdjustLayer(double layer, double value) {
            Material material = TerrainModelManager.Instance.CurrentVisibleModel.Material;
            if (material) {
                material.SetFloat($"_Diffuse{(int)layer}Opacity", (float)(value / 100));
            }
        }

        // Temporary
        [RegisterToBrowser]
        public void GetCurrentLayers(string requestId) {
            // TODO Un-hardcode this data
            Material material = TerrainModelManager.Instance.CurrentVisibleModel.Material;
            IList<object> layers = new List<object>() {
                new Dictionary<string, object>() {
                    { "name", "Test Texture" },
                    { "opacity", (int)(material.GetFloat("_Diffuse1Opacity") * 100) }
                },
                new Dictionary<string, object>() {
                    { "name", "mola_roughness" },
                    { "opacity", (int)(material.GetFloat("_Diffuse2Opacity") * 100) }
                },
                new Dictionary<string, object>() {
                    { "name", "Mars_MGS_MOLA_ClrShade_merge_global_463m" },
                    { "opacity", (int)(material.GetFloat("_Diffuse3Opacity") * 100) }
                }
            };
            ZFBrowserUtils.SendDataResponse(_browser, requestId, layers);
        }

    }

}
