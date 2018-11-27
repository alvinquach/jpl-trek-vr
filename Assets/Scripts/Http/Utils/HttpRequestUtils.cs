using System.Collections.Generic;
using System.Net;

namespace TrekVRApplication.Http {

    public static class HttpRequestUtils {

        public static string AppendParams(string baseUrl, IDictionary<string, string> paramsMap, bool urlEncode = true) {
            string paramsString = GenerateParamsString(paramsMap, urlEncode);
            return AppendParams(baseUrl, paramsString);
        }

        public static string AppendParams(string baseUrl, string paramsString) {
            return string.IsNullOrEmpty(paramsString) ? baseUrl : $"{baseUrl}?{paramsString}";
        }

        public static string GenerateParamsString(IDictionary<string, string> paramsMap, bool urlEncode = true) {
            // TODO Implement this
            return null;
        }


        public static string DecodeUrl(string url) {
            return WebUtility.UrlDecode(url);
        }

        public static string EncodeUrl(string url) {
            return WebUtility.UrlEncode(url);
        }

    }

}