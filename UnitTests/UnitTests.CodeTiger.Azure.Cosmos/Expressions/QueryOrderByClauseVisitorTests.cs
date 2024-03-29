﻿using System;
using System.Linq.Expressions;
using CodeTiger.Azure.Cosmos.Expressions;
using UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes;
using Xunit;

namespace UnitTests.CodeTiger.Azure.Cosmos.Expressions;

public static class QueryOrderByClauseVisitorTests
{
    public static class Visit_Expression
    {
        [Fact]
        public static void CorrectlyHandlesSingleProperty()
        {
            Expression<Func<Sale, decimal>> groupByFunc = x => x.Amount;

            string actual = QueryOrderByClauseVisitor.Visit(groupByFunc);

            Assert.Equal("r.amount", actual);
        }
    }
}
