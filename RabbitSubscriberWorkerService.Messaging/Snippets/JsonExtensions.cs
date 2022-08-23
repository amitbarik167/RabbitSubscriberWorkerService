using Newtonsoft.Json;


namespace RabbitSubscriberWorkerService.Messaging.Snippets
{
    /// <summary>
    /// JsonExtensions
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>string</returns>
        public static string AsJsonString(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }

        /// <summary>
        /// AsJsonStringPretty
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>string</returns>
        public static string AsJsonStringPretty(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}
