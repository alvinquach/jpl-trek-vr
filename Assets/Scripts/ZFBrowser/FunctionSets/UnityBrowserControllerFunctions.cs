using System;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public class UnityBrowserControllerFunctions : UnityBrowserFunctionSet {

        protected override string FunctionsReadyVariable { get; } = "controllerFunctionsReady";

        public UnityBrowserControllerFunctions(Browser browser) : base(browser) {

        }

        [RegisterToBrowser]
        public void StartPrimaryControllerActivity(string activityName) {
            StartControllerActivity(activityName, true);
        }

        [RegisterToBrowser]
        public void StartSecondaryControllerActivity(string activityName) {
            StartControllerActivity(activityName, false);
        }

        // TODO Move this
        [RegisterToBrowser]
        public void NavigateTo(string bbox) {
            Transform planetTransform = TerrainModelManager.Instance.GetGlobalPlanetModelTransform();
            XRInteractablePlanet planet = planetTransform.GetComponent<XRInteractablePlanet>();
            if (planet != null) {
                BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(bbox);
                Vector2 latLon = BoundingBoxUtils.MedianLatLon(boundingBox);
                Camera eye = UserInterfaceManager.Instance.XRCamera;
                planet.NavigateTo(latLon, eye.transform.position);
            }
        }

        private void StartControllerActivity(string activityName, bool primary) {
            ControllerModal controllerModal = primary ?
                UserInterfaceManager.Instance.PrimaryControllerModal :
                UserInterfaceManager.Instance.SecondaryControllerModal;

            if (!Enum.TryParse(activityName, out ControllerModalActivity activity)) {
                Debug.LogError($"{activityName} is not a valid activity.");
                return;
            }

            controllerModal.StartActivity(activity);
        }

    }

}
