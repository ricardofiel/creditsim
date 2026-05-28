using System;
using System.Web;
using System.Web.Http;
using CreditSim.Core.Services;
using CreditSim.Data;
using CreditSim.Web.App_Start;
using CreditSim.Web.Controllers;

namespace CreditSim.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Configure log4net
            log4net.Config.XmlConfigurator.Configure();

            // Register Web API routes and formatters.
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Initialise the customer store (creates the JSON file with seed data if missing).
            var storePath = ConnectionStringProvider.Get();
            var initializer = new DatabaseInitializer(storePath, new CreditScoringService());

            initializer.InitializeAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    System.Diagnostics.Trace.TraceError(
                        "Customer store initialisation failed: " + t.Exception);
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

