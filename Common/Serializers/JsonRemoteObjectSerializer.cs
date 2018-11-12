namespace RemoteCore
{
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public class JsonRemoteObjectSerializer : BaseRemoteObjectSerializer
    {
        /// <summary>
        /// The settings
        /// </summary>
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        /// The string encoder
        /// Will default to UTF8
        /// </summary>
        private readonly Encoding encoder;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRemoteObjectSerializer"/> class.
        /// If no encoder type is passed converter will use UTF8
        /// </summary>
        /// <param name="encoder">The encoder.</param>
        public JsonRemoteObjectSerializer(Encoding encoder = null)
        {
            this.encoder = encoder ?? Encoding.UTF8;
        }

        /// <summary>
        /// Converts the Object into its byte representation.
        /// </summary>
        /// <param name="source">The source object to convert.</param>
        /// <returns></returns>
        public override MessageAsBytes ToByteArray(object source)
        {
            return new MessageAsBytes(encoder.GetBytes(Serialize(source)));
        }

        /// <summary>
        /// Convert the Raw byte stream back into the object.
        /// </summary>
        /// <typeparam name="T">The type to convert the Raw stream into.</typeparam>
        /// <param name="raw">The raw bytes of the object.</param>
        /// <returns></returns>
        public override T ToObject<T>(byte[] raw)
        {
            if (raw == null || raw.Length < 1)
            {
                return default(T);
            }

            var st = encoder.GetString(raw);

            if (st.Contains("$type"))
            {
                var type = JObject.Parse(st)["$type"].ToString();

                if (string.IsNullOrEmpty(type))
                {
                    return default(T);
                }

                return (T)JsonConvert.DeserializeObject(st, Type.GetType(type), settings);
            }

            return (T)JsonConvert.DeserializeObject(st, typeof(T), settings);
        }

        /// <summary>
        /// Serializes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static string Serialize(object source)
        {
            return JsonConvert.SerializeObject(source, settings);
        }

        public static void AddConverter(JsonConverter converter)
        {
            settings.Converters.Add(converter);
        }

        public static void SetCustomContract(IContractResolver resolver)
        {
            settings.ContractResolver = resolver;
        }
    }
}

