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
        public void NavigateToCoordinate(string bbox) {
            Transform planetTransform = TerrainModelManager.Instance.GetGlobalPlanetModelTransform();
            XRInteractablePlanet planet = planetTransform.GetComponent<XRInteractablePlanet>();
            if (planet != null) {
                BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(bbox);
                Vector2 latLon = BoundingBoxUtils.MedianLatLon(boundingBox);
                Camera eye = UserInterfaceManager.Instance.XRCamera;
                planet.NavigateTo(latLon, eye.transform.position);
            }
        }

        // Temporary
        [RegisterToBrowser]
        public void AdjustLayer(double layer, double value) {
            Material material = TerrainModelManager.Instance.CurrentVisibleModel.CurrentMaterial;
            if (material) {
                material.SetFloat($"_Diffuse{(int)layer}", (float)(value / 100));
            }
        }

        // Temporary
        [RegisterToBrowser]
        public void GetCurrentLayers(string requestId) {
            // TODO Un-hardcode this data
            IList<object> layers = new List<object>() {
                new Dictionary<string, object>() {
                    { "name", "Test Texture" },
                    { "opacity", 0 }
                },
                new Dictionary<string, object>() {
                    { "name", "mola_roughness" },
                    { "opacity", 0 }
                },
                new Dictionary<string, object>() {
                    { "name", "Mars_MGS_MOLA_ClrShade_merge_global_463m" },
                    { "opacity", 0 }
                }
            };
            ZFBrowserUtils.SendDataResponse(_browser, requestId, layers);
        }

    }

}
