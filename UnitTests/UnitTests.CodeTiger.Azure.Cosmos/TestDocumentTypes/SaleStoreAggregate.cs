using Newtonsoft.Json;

namespace UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes
{
    internal class SaleStoreAggregate
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [JsonProperty("averageAmount")]
        public decimal AverageAmount { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
    }
}
