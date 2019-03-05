using UnityEngine;

namespace TrekVRApplication {

    public class PrimaryControllerModal : ControllerModal {

        public override bool IsPrimary {
            get { return true; }
        }

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
