using System;
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

        [RegisterToBrowser]
        public void AdjustLayer(int layer, int value) {
            Material material = TerrainModelManager.Instance.CurrentVisibleModel.CurrentMaterial;
            if (material) {
                material.SetFloat($"Diffuse{layer}", value / 100.0f);
            }
        }

    }

}
