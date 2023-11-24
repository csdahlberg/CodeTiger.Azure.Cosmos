using Newtonsoft.Json;

namespace UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes;

internal class SaleSummary
{
    [JsonProperty("averageAmount")]
    public decimal AverageAmount { get; set; }

    [JsonProperty("totalAmount")]
    public decimal TotalAmount { get; set; }

    public SaleSummary()
    {
        AverageAmount = 0.3m;
    }

    public SaleSummary(decimal averageAmount)
    {
        AverageAmount = averageAmount;
    }
}
