using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ZFBrowser {

    public static class ZFBrowserConfig {

        /// <summary>
        ///     Additional command line switches to add to the embedded Chromium browser.
        /// </summary>
        private static readonly string[] AdditionalCommandLineSwitches = new string[] {

            // Enables backdrop-filter CSS
            "--enable-experimental-web-platform-features",



        };

    }

}
