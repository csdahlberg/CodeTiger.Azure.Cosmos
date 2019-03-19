namespace CodeTiger.Azure.Cosmos
{
    internal static partial class AggregateDocumentQueryEvaluator
    {
        private const string QueryPlaceholder = "$(Query)";
        private const string CreateAggregateSeedFunctionPlaceholder = "$(CreateAggregateSeedFunction)";
        private const string CreateAggregateFunctionPlaceholder = "$(CreateAggregateFunction)";
        private const string ShouldStartNewAggregateFunctionPlaceholder = "$(ShouldStartNewAggregateFunction)";
        private const string CreateResultFunctionPlaceholder = "$(CreateResultFunction)";
        private const string StoredProcedureTemplate = @"// ***
// *** DO NOT MODIFY THIS STORED PROCEDURE
// ***
// This stored procedure was automatically created by the CodeTiger.Azure.Cosmos library to facilitate queries
// that aggregate data across multiple documents. It can safely be deleted, and will automatically be re-created if
// it is needed in the future, but any changes made to this stored procedure could cause incorrect behavior that
// will NOT automatically be repaired.

function query(queryState) {
    var collection = getContext().getCollection();
    var query = {
        query: '$(Query)',
        parameters: queryState.parameters
    };

    runQuery(queryState);

    function runQuery(queryState) {
        var requestOptions = { pageSize: -1, continuation: queryState.continuationToken };

        var isAccepted = collection.queryDocuments(
            collection.getSelfLink(), query, requestOptions,
            function (err, feed, options) {
                if (err) {
                    throw err;
                }

                if (feed) {
                    for (var i = 0; i < feed.length; i++) {
                        var current = feed[i];

                        if (queryState.partialAggregate
                            && shouldStartNewAggregate(queryState.previousSourceDocument, current)) {
                            if (!queryState.results) {
                                queryState.results = [];
                            }
                            queryState.results.push(createResult(queryState.partialAggregate));
                            queryState.partialAggregate = null;
                            queryState.returnedResultCount += 1;
                        }

                        if (!queryState.partialAggregate) {
                            queryState.partialAggregate = createAggregateSeed(current);
                        } else {
                            queryState.partialAggregate = createAggregate(queryState.partialAggregate, current);
                        }

                        queryState.previousSourceDocument = current;
                    }
                }

                if (options.continuation
                    && (!queryState.maxResultCount
                        || queryState.returnedResultCount < queryState.maxResultCount)) {
                    queryState.continuationToken = options.continuation;
                    runQuery(queryState);
                } else {
                    queryState.continuationToken = null;
                    if (queryState.partialAggregate) {
                        if (!queryState.results) {
                            queryState.results = [];
                        }
                        queryState.results.push(createResult(queryState.partialAggregate));
                        queryState.partialAggregate = null;
                        queryState.returnedResultCount += 1;
                    }
                    getContext().getResponse().setBody(queryState);
                }
            });

        if (!isAccepted) {
            if (queryState.continuationToken) {
                // The query was likely not accepted because of time or RU limitations. Return the continuation
                // token so the caller can attempt to continue the query.
                getContext().getResponse().setBody(queryState);
            } else {
                throw new Error('The query was not accepted (queryState = ' + JSON.stringify(queryState) + ').');
            }
        }
    }
}

function createAggregateSeed(first) {
    $(CreateAggregateSeedFunction)
}

function createAggregate(aggregate, current) {
    $(CreateAggregateFunction)
}

function shouldStartNewAggregate(previous, current) {
    if (!previous) {
        return false;
    }
    $(ShouldStartNewAggregateFunction)
}

function createResult(aggregate) {
    $(CreateResultFunction)
}";
    }
}
