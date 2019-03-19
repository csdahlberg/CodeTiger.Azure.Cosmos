using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CodeTiger.Azure.Cosmos.Expressions;
using UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes;
using Xunit;

namespace UnitTests.CodeTiger.Azure.Cosmos.Expressions
{
    public static class QueryWhereClauseVisitorTests
    {
        public static class Visit_Expression_IDictionaryOfStringToObject
        {
            [Fact]
            public static void CorrectlyHandlesGreaterThanComparisonOfDecimalProperty()
            {
                Expression<Func<Sale, bool>> whereFunc = x => x.Amount > 3m;

                var parameters = new Dictionary<string, object>();

                string actual = QueryWhereClauseVisitor.Visit(whereFunc, parameters);

                Assert.Equal("r.amount > @p1", actual);
                Assert.Single(parameters);
                Assert.Equal("@p1", parameters.Keys.First());
                Assert.Equal(3m, parameters["@p1"]);
            }
        }
    }
}
