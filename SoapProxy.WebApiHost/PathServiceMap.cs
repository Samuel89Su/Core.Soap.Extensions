using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SoapProxy.WebApiHost
{
    public class PathServiceMap
    {
        public string PathRoot { get; set; }

        public string ServiceClient { get; set; }

        public Dictionary<string, string> PathActions { get; set; } = new Dictionary<string, string>();


        private const string MAP_FILE = "appSettings.json";
        internal static readonly IEnumerable<PathServiceMap> Maps = new List<PathServiceMap>();
        static PathServiceMap()
        {
            if (!File.Exists(MAP_FILE))
            {
                throw new FileNotFoundException("appSettings.json not found!");
            }
            var maps = JsonConvert.DeserializeObject<Dictionary<string, PathServiceMap>>(File.ReadAllText(MAP_FILE));
            foreach (var map in maps)
            {
                map.Value.PathRoot = "/" + map.Key;
                var actions = new Dictionary<string, string>();
                foreach (var action in map.Value.PathActions)
                {
                    actions[map.Value.PathRoot + "/" + action.Key] = action.Value;
                }
                map.Value.PathActions = actions;
            }

            Maps = maps.Values;
        }
    }
}
