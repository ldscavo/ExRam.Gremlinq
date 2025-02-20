﻿using ExRam.Gremlinq.Core.Serialization;
using ExRam.Gremlinq.Core.Steps;
using ExRam.Gremlinq.Core.Transformation;
using ExRam.Gremlinq.Providers.Core;
using ExRam.Gremlinq.Providers.CosmosDb;
using Gremlin.Net.Driver;
using Gremlin.Net.Process.Traversal;
using static ExRam.Gremlinq.Core.Transformation.ConverterFactory;

namespace ExRam.Gremlinq.Core
{
    public static class ConfigurableGremlinQuerySourceExtensions
    {
        private sealed class CosmosDbConfigurator : ICosmosDbConfigurator
        {
            private readonly string? _authKey;
            private readonly string? _graphName;
            private readonly string? _databaseName;
            private readonly WebSocketProviderConfigurator _webSocketConfigurator;

            public CosmosDbConfigurator() : this(new WebSocketProviderConfigurator(), null, null, null)
            {
            }

            private CosmosDbConfigurator(WebSocketProviderConfigurator webSocketProviderConfigurator, string? databaseName, string? graphName, string? authKey)
            {
                _authKey = authKey;
                _graphName = graphName;
                _databaseName = databaseName;
                _webSocketConfigurator = webSocketProviderConfigurator;
            }

            public ICosmosDbConfigurator OnDatabase(string databaseName) => new CosmosDbConfigurator(
                _webSocketConfigurator
                    .ConfigureServer(server => server.WithUsername($"/dbs/{databaseName}/colls/{_graphName ?? string.Empty}")),
                databaseName,
                _graphName,
                _authKey);

            public ICosmosDbConfigurator OnGraph(string graphName) => new CosmosDbConfigurator(
                _webSocketConfigurator
                    .ConfigureServer(server => server.WithUsername($"/dbs/{_databaseName ?? string.Empty}/colls/{graphName}")),
                _databaseName,
                graphName,
                _authKey);

            public ICosmosDbConfigurator AuthenticateBy(string authKey) => new CosmosDbConfigurator(
                _webSocketConfigurator
                    .ConfigureServer(server => server.WithPassword(authKey)),
                _databaseName,
                _graphName,
                authKey);

            public ICosmosDbConfigurator ConfigureServer(Func<GremlinServer, GremlinServer> transformation) => new CosmosDbConfigurator(
                _webSocketConfigurator.ConfigureServer(transformation),
                _databaseName,
                _graphName,
                _authKey);

            public ICosmosDbConfigurator ConfigureClientFactory(Func<IGremlinClientFactory, IGremlinClientFactory> transformation) => new CosmosDbConfigurator(
                _webSocketConfigurator.ConfigureClientFactory(transformation),
                _databaseName,
                _graphName,
                _authKey);

            public ICosmosDbConfigurator ConfigureQuerySource(Func<IGremlinQuerySource, IGremlinQuerySource> transformation) => new CosmosDbConfigurator(
                _webSocketConfigurator.ConfigureQuerySource(transformation),
                _databaseName,
                _graphName,
                _authKey);

            public IGremlinQuerySource Transform(IGremlinQuerySource source) => _webSocketConfigurator.Transform(source);
        }

        private class WorkaroundOrder : EnumWrapper, IComparator
        {
            public static readonly WorkaroundOrder Incr = new("incr");
            public static readonly WorkaroundOrder Decr = new("decr");

            private WorkaroundOrder(string enumValue)  : base("Order", enumValue)
            {
            }
        }

        private static readonly NotStep NoneWorkaround = new(IdentityStep.Instance);

        public static IGremlinQuerySource UseCosmosDb(this IConfigurableGremlinQuerySource source, Func<ICosmosDbConfigurator, IGremlinQuerySourceTransformation> configuratorTransformation)
        {
            return configuratorTransformation
                .Invoke(new CosmosDbConfigurator())
                .Transform(source
                    .ConfigureEnvironment(environment => environment
                        .ConfigureFeatureSet(featureSet => featureSet
                            .ConfigureGraphFeatures(_ => GraphFeatures.Transactions | GraphFeatures.Persistence | GraphFeatures.ConcurrentAccess)
                            .ConfigureVariableFeatures(_ => VariableFeatures.BooleanValues | VariableFeatures.IntegerValues | VariableFeatures.ByteValues | VariableFeatures.DoubleValues | VariableFeatures.FloatValues | VariableFeatures.IntegerValues | VariableFeatures.LongValues | VariableFeatures.StringValues)
                            .ConfigureVertexFeatures(_ => VertexFeatures.RemoveVertices | VertexFeatures.MetaProperties | VertexFeatures.AddVertices | VertexFeatures.MultiProperties | VertexFeatures.StringIds | VertexFeatures.UserSuppliedIds | VertexFeatures.AddProperty | VertexFeatures.RemoveProperty)
                            .ConfigureVertexPropertyFeatures(_ => VertexPropertyFeatures.StringIds | VertexPropertyFeatures.UserSuppliedIds | VertexPropertyFeatures.RemoveProperty | VertexPropertyFeatures.BooleanValues | VertexPropertyFeatures.ByteValues | VertexPropertyFeatures.DoubleValues | VertexPropertyFeatures.FloatValues | VertexPropertyFeatures.IntegerValues | VertexPropertyFeatures.LongValues | VertexPropertyFeatures.StringValues)
                            .ConfigureEdgeFeatures(_ => EdgeFeatures.AddEdges | EdgeFeatures.RemoveEdges | EdgeFeatures.StringIds | EdgeFeatures.UserSuppliedIds | EdgeFeatures.AddProperty | EdgeFeatures.RemoveProperty)
                            .ConfigureEdgePropertyFeatures(_ => EdgePropertyFeatures.Properties | EdgePropertyFeatures.BooleanValues | EdgePropertyFeatures.ByteValues | EdgePropertyFeatures.DoubleValues | EdgePropertyFeatures.FloatValues | EdgePropertyFeatures.IntegerValues | EdgePropertyFeatures.LongValues | EdgePropertyFeatures.StringValues))
                        .ConfigureOptions(options => options
                            .SetValue(GremlinqOption.VertexProjectionSteps, Traversal.Empty)
                            .SetValue(GremlinqOption.EdgeProjectionSteps, Traversal.Empty)
                            .SetValue(GremlinqOption.VertexPropertyProjectionSteps, Traversal.Empty)
                            .SetValue(GremlinqOption.PreferGroovySerialization, true))
                        .ConfigureNativeTypes(nativeTypes => nativeTypes
                            .Remove(typeof(byte[]))
                            .Remove(typeof(TimeSpan)))
                        .UseGraphSon2()
                        .ConfigureSerializer(serializer => serializer
                            .Add(ConverterFactory
                                .Create<CosmosDbKey, string>((key, _, _) => key.Id)
                                .AutoRecurse<string>())
                            .Add(ConverterFactory
                                .Create<CosmosDbKey, string[]>((key, _, _) => key.PartitionKey is { } partitionKey
                                    ? new[] { partitionKey, key.Id }
                                    : default)
                                .AutoRecurse<string[]>())
                            .Add(ConverterFactory
                                .Create<FilterStep.ByTraversalStep, WhereTraversalStep>(static (step, _, _) => new WhereTraversalStep(
                                    step.Traversal.Count > 0 && step.Traversal[0] is AsStep
                                        ? new MapStep(step.Traversal)
                                        : step.Traversal)))
                            .Add(ConverterFactory
                                .Create<HasKeyStep, WhereTraversalStep>((step, _, _) => step.Argument is P p && (!p.OperatorName.Equals("eq", StringComparison.OrdinalIgnoreCase))
                                    ? new WhereTraversalStep(Traversal.Empty.Push(
                                        KeyStep.Instance,
                                        new IsStep(p)))
                                    : default))
                            .Add(ConverterFactory
                                .Create<NoneStep, NotStep>((_, _, _) => NoneWorkaround))
                            .Add(ConverterFactory
                                .Create<SkipStep, RangeStep>((step, _, _) => new RangeStep(step.Count, -1, step.Scope)))
                            .Add(Guard<LimitStep>(step =>
                            {
                                if (step.Count > int.MaxValue)
                                    throw new ArgumentOutOfRangeException(nameof(step), "CosmosDb doesn't currently support values for 'Limit' outside the range of a 32-bit-integer.");
                            }))
                            .Add(Guard<TailStep>(step =>
                            {
                                if (step.Count > int.MaxValue)
                                    throw new ArgumentOutOfRangeException(nameof(step), "CosmosDb doesn't currently support values for 'Tail' outside the range of a 32-bit-integer.");
                            }))
                            .Add(Guard<RangeStep>(step =>
                            {
                                if (step.Lower > int.MaxValue || step.Upper > int.MaxValue)
                                    throw new ArgumentOutOfRangeException(nameof(step), "CosmosDb doesn't currently support values for 'Range' outside the range of a 32-bit-integer.");
                            }))
                            .Add(ConverterFactory
                                .Create<Order, WorkaroundOrder>((order, _, _) => order.Equals(Order.Asc)
                                    ? WorkaroundOrder.Incr
                                    : order.Equals(Order.Desc)
                                        ? WorkaroundOrder.Decr
                                        : default)
                                .AutoRecurse<WorkaroundOrder>()))));
        }
    }
}
