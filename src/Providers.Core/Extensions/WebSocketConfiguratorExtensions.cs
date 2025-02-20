﻿using Gremlin.Net.Driver;

namespace ExRam.Gremlinq.Providers.Core
{
    public static class WebSocketConfiguratorExtensions
    {
        public static TConfigurator At<TConfigurator>(this IWebSocketProviderConfigurator<TConfigurator> builder, string uri)
            where TConfigurator : IWebSocketProviderConfigurator<TConfigurator> => builder.At(new Uri(uri));

        public static TConfigurator AtLocalhost<TConfigurator>(this IWebSocketProviderConfigurator<TConfigurator> builder)
            where TConfigurator : IWebSocketProviderConfigurator<TConfigurator> => builder.At(new Uri("ws://localhost:8182"));

        public static TConfigurator ConfigureMessageSerializer<TConfigurator>(this IWebSocketProviderConfigurator<TConfigurator> configurator, Func<IMessageSerializer, IMessageSerializer> transformation)
            where TConfigurator : IWebSocketProviderConfigurator<TConfigurator> => configurator
                .ConfigureClientFactory(factory => GremlinClientFactory
                    .Create((environment, server, maybeSerializer, poolSettings, optionsTransformation, sessionId) => factory.Create(environment, server, maybeSerializer is { } serializer ? transformation(serializer) : maybeSerializer, poolSettings, optionsTransformation, sessionId)));

        public static TConfigurator At<TConfigurator>(this IWebSocketProviderConfigurator<TConfigurator> configurator, Uri uri)
            where TConfigurator : IWebSocketProviderConfigurator<TConfigurator> => configurator
                .ConfigureServer(server => server
                    .WithUri(uri));

        public static TConfigurator AuthenticateBy<TConfigurator>(this IWebSocketProviderConfigurator<TConfigurator> configurator, string username, string password)
            where TConfigurator : IWebSocketProviderConfigurator<TConfigurator> => configurator
                .ConfigureServer(server => server
                    .WithUsername(username)
                    .WithPassword(password));
    }
}
