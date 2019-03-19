using Newtonsoft.Json;

namespace UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes
{
    internal class SaleAggregate
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("averageAmount")]
        public decimal AverageAmount { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        public SaleAggregate()
        {
        }

        public SaleAggregate(int count)
        {
            Count = count;
        }
    }
}
