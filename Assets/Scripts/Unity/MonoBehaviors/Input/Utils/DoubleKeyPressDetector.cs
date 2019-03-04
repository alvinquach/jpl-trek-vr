using System;
using UnityEngine;

namespace TrekVRApplication {

    public class DoubleKeyPressDetector {

        private long _timeAtLastKeyDown = -1;

        private long _timeAtLastKeyPress = -1;

        /// <summary>
        ///     Maximum delay (in milliseconds) between two key
        ///     presses to be registered as a double press.
        /// </summary>
        public long MaxKeyPressInterval { get; set; } = 400;

        /// <summary>
        ///     Maximum duration (in milliseconds) the key can
        ///     be held down to be considred a click.
        /// </summary>
        public long MaxKeyPressDuration { get; set; } = 300;

        public event Action OnDoubleKeyPress = () => { };

        public void RegisterKeyDown() {
            _timeAtLastKeyDown = Now();
        }

        public void RegisterKeyUp() {
            if (_timeAtLastKeyDown < 0) {
                return;
            }
            long now = Now();

            // New key press registered.
            if (now - _timeAtLastKeyDown <= MaxKeyPressDuration) {

                // If this is the first keypress, or the time since the last
                // key press took to long, then register as first keypress.
                if (_timeAtLastKeyPress < 0 || now - _timeAtLastKeyPress > MaxKeyPressInterval) {
                    _timeAtLastKeyPress = now;
                }

                // If the key was press in time, then call the double keypress action.
                else if (now - _timeAtLastKeyPress <= MaxKeyPressInterval) {
                    _timeAtLastKeyPress = -1;
                    OnDoubleKeyPress();
                }
            }

            _timeAtLastKeyDown = -1;
        }

        private long Now() {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

    }

}
