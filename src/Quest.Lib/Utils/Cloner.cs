using Newtonsoft.Json;

namespace Quest.Lib.Utils
{
    public static class Cloner
    {
        public static T CloneJson<T>(this object source)
        {
            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, ReferenceLoopHandling= ReferenceLoopHandling.Ignore };

            var json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json, deserializeSettings);
        }
    }
}
