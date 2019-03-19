// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CodeTiger.Reliability",
    "CT2001:Types with disposable state should implement IDisposable", Scope = "type",
    Target = "~T:CodeTiger.Azure.Cosmos.AggregateQueryProvider",
    Justification = "The DocumentClient instance should be shared ")]
[assembly: SuppressMessage("CodeTiger.Reliability",
    "CT2007:Empty catch blocks should not be used.", Scope = "member",
    Target = "~M:CodeTiger.Azure.Cosmos.AggregateDocumentQueryExecutionContext`1"
        +".ExecuteNextAsync(System.Threading.CancellationToken)"
        + "~System.Threading.Tasks.Task{CodeTiger.Azure.Cosmos.AggregateQueryResponse{`0}}",
    Justification = "<Pending>")]
