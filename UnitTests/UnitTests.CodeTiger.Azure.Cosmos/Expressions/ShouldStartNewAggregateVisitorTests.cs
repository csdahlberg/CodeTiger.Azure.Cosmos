using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CodeTiger.Azure.Cosmos.Expressions;
using UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes;
using Xunit;

namespace UnitTests.CodeTiger.Azure.Cosmos.Expressions
{
    public static class ShouldStartNewAggregateVisitorTests
    {
        public static class Visit_Expression
        {
            [Fact]
            public static void CorrectlyHandlesSingleProperty()
            {
                Expression<Func<Sale, string>> groupByFunc = x => x.StoreId;

                string actual = ShouldStartNewAggregateVisitor.Visit(groupByFunc);

                Assert.Equal("current.storeId != previous.storeId", actual);
            }
        }
    }
}
