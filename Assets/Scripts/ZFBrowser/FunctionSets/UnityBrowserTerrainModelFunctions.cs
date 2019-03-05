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

    }

}
