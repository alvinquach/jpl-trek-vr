using UnityEngine;

namespace TrekVRApplication {

    public class PrimaryControllerModal : ControllerModal {

        public override bool IsPrimary {
            get { return true; }
        }

        #region Controller event handlers

        protected override void MenuButtonClickedHandler(object sender, ClickedEventArgs e) {
            UserInterfaceManager userInterfaceManager = UserInterfaceManager.Instance;
            if (userInterfaceManager.MainModal.Visible) {
                userInterfaceManager.MainModal.Visible = false;
            }
            else {
                userInterfaceManager.SecondaryControllerModal.StartActivity(ControllerModalActivity.Default);
                userInterfaceManager.MainModal.Visible = true;
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
