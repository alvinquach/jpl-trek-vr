using UnityEngine;

namespace TrekVRApplication {

    public class PrimaryControllerModal : ControllerModal {

        public override bool IsPrimary {
            get { return true; }
        }

        #region Controller event handlers

        protected override void MenuButtonPressedHandler(object sender, ClickedEventArgs e) {
            UserInterfaceManager userInterfaceManager = UserInterfaceManager.Instance;
            if (userInterfaceManager.MainModal.Visible) {
                // TODO Navigate backwards in modal menu.
            }
            else {
                userInterfaceManager.SecondaryControllerModal.StartActivity(ControllerModalActivity.Default);
                userInterfaceManager.MainModal.Visible = true;
            }
        }

        protected override void MenuButtonLongPressedHandler(object sender, ClickedEventArgs e) {
            UserInterfaceManager userInterfaceManager = UserInterfaceManager.Instance;
            MainModal mainModal = userInterfaceManager.MainModal;
            if (mainModal.Visible) {

                // FIXME Need to set the mode for all the terrain models, not just the planet.
                XRInteractablePlanet planet = TerrainModelManager.Instance.GetComponentFromCurrentModel<XRInteractablePlanet>();
                planet.SwitchToMode(XRInteractablePlanetMode.Navigate);

                mainModal.Visible = false;
                mainModal.NavigateToRootMenu();
            }
            else {
                userInterfaceManager.SecondaryControllerModal.StartActivity(ControllerModalActivity.Default);
                mainModal.Visible = true;
            }
        }

        #endregion

        public override void StartActivity(ControllerModalActivity activity) {
            if (activity == CurrentActivity) {
                return;
            }
            if (!activity.IsAvailableForPrimary()) {
                Debug.LogError($"Activity {activity} is not available for the primary controller.");
                return;
            }
            base.StartActivity(activity);
        }

    }
}
