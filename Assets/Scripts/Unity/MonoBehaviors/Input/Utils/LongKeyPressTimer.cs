namespace TrekVRApplication {

    public class LongKeyPressTimer : KeyPressTimer {

        private long _timeAtPressStart = -1;

        /// <summary>
        ///     Minimum time (in milliseconds) that the key has to
        ///     be held down to be registered as a long hold/press.
        /// </summary>
        public long MinKeyPressInterval { get; set; } = 750;

        public override void RegisterKeyDown() {
            _timeAtPressStart = Now();
        }

        public override void RegisterKeyUp() {
            _timeAtPressStart = -1;
        }

        public override void Update() {
            if (_timeAtPressStart < 0) {
                return;
            }
            if (Now() - _timeAtPressStart >= MinKeyPressInterval) {
                _timeAtPressStart = -1;
                InvokeActionSuccess();
            }
        }


    }

}
