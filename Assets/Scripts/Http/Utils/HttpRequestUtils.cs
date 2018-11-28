using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TrekVRApplication {

    public static class HttpRequestUtils {

        public static string AppendParams(string baseUrl, IDictionary<string, string> paramsMap, bool urlEncodeParams = true) {
            string paramsString = GenerateParamsString(paramsMap, urlEncodeParams);
            return AppendParams(baseUrl, paramsString);
        }

        public static string AppendParams(string baseUrl, string paramsString) {
            return string.IsNullOrEmpty(paramsString) ? baseUrl : $"{baseUrl}?{paramsString}";
        }

        public static string GenerateParamsString(IDictionary<string, string> paramsMap, bool urlEncode = true) {
            if (paramsMap.Count == 0) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (KeyValuePair<string, string> kv in paramsMap) {
                if (index > 0) {
                    sb.Append("&");
                }
                sb.Append(urlEncode ? $"{EncodeUrl(kv.Key)}={EncodeUrl(kv.Value)}" : $"{kv.Key}={kv.Value}");
                index++;
            }
            return sb.ToString();
        }

        public static string DecodeUrl(string url) {
            return WebUtility.UrlDecode(url);
        }

        public static string EncodeUrl(string url) {
            return WebUtility.UrlEncode(url);
        }

    }

}