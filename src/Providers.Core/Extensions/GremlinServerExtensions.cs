﻿using _GremlinServer = Gremlin.Net.Driver.GremlinServer;

namespace Gremlin.Net.Driver
{
    public static class GremlinServerExtensions
    {
        public static _GremlinServer WithHost(this _GremlinServer server, string host) => new(
            host,
            server.Uri.Port,
            server.IsSslEnabled(),
            server.Username,
            server.Password);

        public static _GremlinServer WithPort(this _GremlinServer server, int port) => new(
            server.Uri.Host,
            port,
            server.IsSslEnabled(),
            server.Username,
            server.Password);

        public static _GremlinServer WithUsername(this _GremlinServer server, string username) => new(
            server.Uri.Host,
            server.Uri.Port,
            server.IsSslEnabled(),
            username,
            server.Password);

        public static _GremlinServer WithPassword(this _GremlinServer server, string password) => new(
            server.Uri.Host,
            server.Uri.Port,
            server.IsSslEnabled(),
            server.Username,
            password);

        public static _GremlinServer WithSslEnabled(this _GremlinServer server, bool sslEnabled) => new(
            server.Uri.Host,
            server.Uri.Port,
            sslEnabled,
            server.Username,
            server.Password);

        internal static _GremlinServer WithUri(this _GremlinServer server, Uri uri)
        {
            if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
                throw new ArgumentException($"The {nameof(Uri)} may not contain an {nameof(Uri.AbsolutePath)}.", nameof(uri));

            return new _GremlinServer(
                uri.Host,
                uri.Port,
                uri.IsSslEnabled(),
                server.Username,
                server.Password);
        }

        private static bool IsSslEnabled(this Uri uri) => uri.Scheme is "wss" or "https";

        private static bool IsSslEnabled(this _GremlinServer server) => server.Uri.IsSslEnabled();
    }
}
