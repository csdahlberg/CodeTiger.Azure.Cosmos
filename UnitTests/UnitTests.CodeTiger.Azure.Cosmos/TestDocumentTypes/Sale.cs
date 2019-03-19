using System;
using Newtonsoft.Json;

namespace UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes
{
    internal class Sale
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
