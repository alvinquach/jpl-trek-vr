using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TrekVRApplication {

    public static class JsonConfig {

        private static JsonSerializerSettings _serializerSettings;
        public static JsonSerializerSettings SerializerSettings {
            get {
                if (_serializerSettings == null) {
                    _serializerSettings = new JsonSerializerSettings();
                    _serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    _serializerSettings.Converters.Add(new StringEnumConverter());
                }
                return _serializerSettings;
            }
        }

    }

}
