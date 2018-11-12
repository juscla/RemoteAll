namespace RemoteCore
{
    using System;
    using Newtonsoft.Json;

    public abstract class NonGenericConstructorConverter<T> : JsonConverter
    {
        public sealed override bool CanWrite => false;

        public sealed override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

        public abstract T Parse(JsonReader reader);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => this.Parse(reader);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}

