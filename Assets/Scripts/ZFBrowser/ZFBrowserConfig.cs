using ZenFulcrum.EmbeddedBrowser;

namespace TrekVRApplication {

    public static class ZFBrowserConfig {

        private static bool _flagsAdded = false;

        /// <summary>
        ///     Additional command line switches to add to the embedded Chromium browser.
        /// </summary>
        private static readonly string[] AdditionalCommandLineSwitches = new string[] {

            // Enables backdrop-filter CSS
            "--enable-experimental-web-platform-features",

            "--disable-web-security", "--user-data-dir"

        };

        public static void AddFlags() {
            if (_flagsAdded) {
                return;
            }
            foreach(string flag in AdditionalCommandLineSwitches) {
                BrowserNative.commandLineSwitches.Add(flag);
            }
            _flagsAdded = true;
        }

    }

}
