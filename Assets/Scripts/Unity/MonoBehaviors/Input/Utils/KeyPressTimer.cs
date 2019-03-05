using System;

namespace TrekVRApplication {

    public abstract class KeyPressTimer {

        public event Action OnActionSuccess = () => { };

        public abstract void RegisterKeyDown();

        public abstract void RegisterKeyUp();

        public virtual void Update() { }

        protected void InvokeActionSuccess() {
            OnActionSuccess.Invoke();
        }

        protected long Now() {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

    }

}
