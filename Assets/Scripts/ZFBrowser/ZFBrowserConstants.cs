namespace TrekVRApplication {

    public class ZFBrowserConstants {

        public const string BaseUrl = "localhost:4200";
        //public const string BaseUrl = "localGame://index.html";

        public const string AngularGlobalData = "AngularGlobalVariables";
        public const string UnityGlobalData = "UnityGlobalVariables";

        public static readonly string AngularGlobalObjectPath = $"window['{AngularGlobalData}']";
        public static readonly string UnityGlobalObjectPath = $"window['{UnityGlobalData}']";

    }

}
