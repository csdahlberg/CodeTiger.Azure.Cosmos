using System;

namespace UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes
{
    internal class Container<T>
    {
        public T Value { get; set; }
    }
}
