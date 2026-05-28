using System;
using System.IO;
using System.Web;
using System.Web.Http;
using CreditSim.Core.Services;
using CreditSim.Data;
using CreditSim.Web.App_Start;

namespace CreditSim.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Point |DataDirectory| at the repository's data/ folder so the
            // connection string "Data Source=|DataDirectory|\creditsim.db" resolves correctly.
            // The web project lives at dotnet/src/CreditSim.Web/, so we go up three levels.
            var dataDir = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "data"));
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);

            // Configure log4net
            log4net.Config.XmlConfigurator.Configure();

            // Register Web API routes and formatters
            WebApiConfig.Register(GlobalConfiguration.Configuration);

            // Initialise database schema + seed data asynchronously
            var connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["CreditSimDb"].ConnectionString;

            var initializer = new DatabaseInitializer(
                connectionString,
                new CreditScoringService());

            // Fire-and-forget on startup; errors are logged via Trace
            initializer.InitializeAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    System.Diagnostics.Trace.TraceError(
                        "Database initialisation failed: " + t.Exception);
            });
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            log4net.LogManager.GetLogger(typeof(Global))
                .Error("Unhandled application error", ex);
        }
    }
}
