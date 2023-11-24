using Newtonsoft.Json;

namespace UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes;

internal class Container<T>
{
    [JsonProperty("value")]
    public T Value { get; set; }
}
