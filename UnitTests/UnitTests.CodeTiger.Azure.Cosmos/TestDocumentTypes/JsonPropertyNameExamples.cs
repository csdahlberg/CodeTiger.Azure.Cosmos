using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes
{
    internal class JsonPropertyNameExamples
    {
        public string NoAttribute { get; set; }

        [JsonProperty("attributeWithNormalArgument")]
        public string AttributeWithNormalArgument { get; set; }

        [JsonProperty(propertyName: "attributeWithNamedArgument")]
        public string AttributeWithNamedArgument { get; set; }

        [JsonProperty(PropertyName = "attributeWithNamedProperty")]
        public string AttributeWithNamedProperty { get; set; }
    }
}
