using System.Configuration;
using System.Web.Hosting;

namespace CreditSim.Web.Controllers
{
    /// <summary>
    /// Provides the resolved absolute path to the file-based customer store.
    /// </summary>
    internal static class ConnectionStringProvider
    {
        public static string Get()
        {
            var raw = ConfigurationManager.AppSettings["CustomerStorePath"]
                      ?? "~/App_Data/customers.json";
            return HostingEnvironment.MapPath(raw) ?? raw;
        }
    }
}


