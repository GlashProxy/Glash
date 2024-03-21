namespace GlashClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var url = "qp.ws://61.145.61.59:10000/server/app2/Index";
            url = url.Substring(0, url.LastIndexOf("/"));

            Quick.Protocol.QpAllClients.RegisterUriSchema();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}