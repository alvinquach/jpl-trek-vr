using ZenFulcrum.EmbeddedBrowser;
using static TrekVRApplication.ServiceManager;

namespace TrekVRApplication {

    public class UnityBrowserToolsFunctions : UnityBrowserFunctionSet {

        protected override string FunctionsReadyVariable { get; } = "toolsFunctionsReady";

        public UnityBrowserToolsFunctions(Browser browser) : base(browser) {

        }

        [RegisterToBrowser]
        public void GetDistance(string points, string requestId) {
            ToolsWebService.GetDistance(points, res => {
                ZFBrowserUtils.SendDataResponse(_browser, requestId, res);
            });
        }

    }

}
