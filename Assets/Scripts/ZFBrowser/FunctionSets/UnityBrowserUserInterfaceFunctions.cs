using System;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public class UnityBrowserUserInterfaceFunctions : UnityBrowserFunctionSet {

        protected override string FunctionsReadyVariable { get; } = "userInterfaceFunctionsReady";

        public UnityBrowserUserInterfaceFunctions(Browser browser) : base(browser) {

        }

        [RegisterToBrowser]
        public void StartPrimaryControllerActivity(string activityName) {
            StartControllerActivity(activityName, true);
        }

        [RegisterToBrowser]
        public void StartSecondaryControllerActivity(string activityName) {
            StartControllerActivity(activityName, false);
        }

        [RegisterToBrowser]
        public void SetMainModalVisiblity(bool? visible) {
            if (visible == null) {
                return;
            }
            bool value = (bool)visible;
            UserInterfaceManager.Instance.MainModal.Visible = value;
            
            // FIXME Need to set the mode for all the terrain models, not just the planet.
            if (!value) {
                XRInteractableGlobeTerrain globe = TerrainModelManager.Instance.GetComponentFromCurrentModel<XRInteractableGlobeTerrain>();
                globe.SwitchToActivity(XRInteractableTerrainActivity.Default);
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
