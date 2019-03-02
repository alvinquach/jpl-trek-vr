using UnityEngine;

namespace TrekVRApplication {

    public class SecondaryControllerModal : ControllerModal {

        public override bool IsPrimary {
            get { return false; }
        }

        #region Controller event handlers

        protected override void MenuButtonClickedHandler(object sender, ClickedEventArgs e) {
            MainModal mainModal = UserInterfaceManager.Instance.MainModal;
            switch (CurrentActivity) {
                case ControllerModalActivity.Default:
                    mainModal.Visible = !mainModal.Visible;
                    break;
                case ControllerModalActivity.BBoxSelection:
                    XRInteractablePlanet planet = TerrainModelManager.Instance.GetComponentFromCurrentModel<XRInteractablePlanet>();
                    planet.CancelSelection();
                    break;
                case ControllerModalActivity.BookmarkResults:
                case ControllerModalActivity.ProductResults:
                    mainModal.Visible = true;
                    StartActivity(ControllerModalActivity.Default);
                    break;
            }
        }

        #endregion

        public override void StartActivity(ControllerModalActivity activity) {
            if (activity == CurrentActivity) {
                return;
            }
            if (!activity.IsAvailableForSecondary()) {
                Debug.LogError($"Activity {activity} is not available for the secondary controller.");
                return;
            }
            base.StartActivity(activity);
        }

    }
}
