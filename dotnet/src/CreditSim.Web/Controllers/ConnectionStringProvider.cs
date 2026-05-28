using System.Configuration;

namespace CreditSim.Web.Controllers
{
    /// <summary>
    /// Provides the SQLite connection string, resolved from Web.config.
    /// The |DataDirectory| token is set to the repository's data/ folder in Global.asax.
    /// </summary>
    internal static class ConnectionStringProvider
    {
        public static string Get()
        {
            return ConfigurationManager.ConnectionStrings["CreditSimDb"].ConnectionString;
        }
    }
}
