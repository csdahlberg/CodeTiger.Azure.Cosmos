using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using CodeTiger.Azure.Cosmos.Expressions;
using UnitTests.CodeTiger.Azure.Cosmos.TestDocumentTypes;
using Xunit;

namespace UnitTests.CodeTiger.Azure.Cosmos.Expressions
{
    public static class CreateAggregateVisitorTests
    {
        public static class Visit_Expression
        {
            [Fact]
            public static void ReturnsObjectLiteralForNewObjectExpressionWithoutArguments()
            {
                Expression<Func<SaleAggregate, Sale, SaleAggregate>> createAggregateFunc = (aggregate, current)
                    => new SaleAggregate();

                string actual = CreateAggregateVisitor.Visit(createAggregateFunc);

                Assert.Equal("{ \"count\": 0, \"averageAmount\": 0.0, \"totalAmount\": 0.0 }", actual);
            }

            [Fact]
            public static void ReturnsObjectLiteralForNewObjectExpressionWithArguments()
            {
                Expression<Func<SaleAggregate, Sale, SaleAggregate>> createAggregateFunc = (aggregate, current)
                    => new SaleAggregate(1);

                string actual = CreateAggregateVisitor.Visit(createAggregateFunc);

                Assert.Equal("{ \"count\": 1, \"averageAmount\": 0.0, \"totalAmount\": 0.0 }", actual);
            }

            [Fact]
            public static void ReturnsObjectLiteralForMemberInitializationExpressionWithoutArguments()
            {
                Expression<Func<SaleAggregate, Sale, SaleAggregate>> createAggregateFunc = (aggregate, current)
                    => new SaleAggregate { TotalAmount = 2.5m };

                string actual = CreateAggregateVisitor.Visit(createAggregateFunc);

                Assert.Equal("{ \"count\": 0, \"averageAmount\": 0.0, \"totalAmount\": 2.5 }", actual);
            }

            [Fact]
            public static void ReturnsObjectLiteralForMemberInitializationExpressionWithArguments()
            {
                Expression<Func<SaleAggregate, Sale, SaleAggregate>> createAggregateFunc = (aggregate, current)
                    => new SaleAggregate(1) { TotalAmount = 2.5m };

                var actual = CreateAggregateVisitor.Visit(createAggregateFunc);

                Assert.Equal("{ \"count\": 1, \"averageAmount\": 0.0, \"totalAmount\": 2.5 }", actual);
            }

            [Fact]
            public static void CorrectlySetsPropertyValuesForPrimitiveTypeConstants()
            {
                Expression<Func<PrimitiveTypes, PrimitiveTypes, PrimitiveTypes>> createAggregateFunc
                    = (aggregate, current) => new PrimitiveTypes
                    {
                        Boolean = true,
                        Byte = 2,
                        Char = 'x',
                        Double = 3.14159,
                        Int16 = -3,
                        Int32 = 58,
                        Int64 = 249,
                        SByte = -3,
                        Single = 2.158f,
                        UInt16 = 4,
                        UInt32 = 7,
                        UInt64 = 12,
                    };

                string actual = CreateAggregateVisitor.Visit(createAggregateFunc);

                Assert.Equal("{ \"Boolean\": true, \"Byte\": 2, \"Char\": \"x\", \"Double\": 3.14159,"
                    + " \"Int16\": -3, \"Int32\": 58, \"Int64\": 249, \"SByte\": -3, \"Single\": 2.158,"
                    + " \"UInt16\": 4, \"UInt32\": 7, \"UInt64\": 12 }",
                    actual);
            }

            [Fact]
            public static void CorrectlySetsPropertyValuesForDateTimeConstant()
            {
                var dateTime = DateTime.Now;

                Expression<Func<Container<DateTime>, Container<DateTime>, Container<DateTime>>> createAggregateFunc
                    = (aggregate, current) => new Container<DateTime> { Value = dateTime };

                string actual = CreateAggregateVisitor.Visit(createAggregateFunc);

                Assert.Equal("{ \"value\": \"" + dateTime.ToString("O", CultureInfo.InvariantCulture) + "\" }",
                    actual);
            }
        }
    }
}
