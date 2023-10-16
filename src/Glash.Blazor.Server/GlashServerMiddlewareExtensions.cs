using Glash.Blazor.Server;
using Glash.Server;
using Quick.EntityFrameworkCore.Plus;
using Quick.Protocol.WebSocket.Server.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class GlashServerMiddlewareExtensions
    {
        private static QpWebSocketServer qpServer;

        public static IApplicationBuilder UseGlashServer(
            this IApplicationBuilder app,
            string path,
            string password,
            int maxTunnelCount = 100)
        {
            var serverOptions = new QpWebSocketServerOptions()
            {
                Path = path,
                Password = password,
                ServerProgram = "Glash.Server"
            };
            Global.Instance.Init(serverOptions, maxTunnelCount);
            app.UseQuickProtocol(serverOptions, out qpServer);
            qpServer.Start();
            return app;
        }
    }
}