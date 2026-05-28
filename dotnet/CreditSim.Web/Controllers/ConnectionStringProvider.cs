using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace CreditSim.Web.Controllers
{
    /// <summary>
    /// Provides the resolved absolute path to the file-based customer store.
    /// </summary>
    internal static class ConnectionStringProvider
    {
        public static string Get(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var raw = configuration["CustomerStorePath"] ?? "App_Data/customers.json";

            // Strip ASP.NET-style "~/" prefix for backwards compatibility with the
            // original Web.config value.
            if (raw.StartsWith("~/", StringComparison.Ordinal))
                raw = raw.Substring(2);

            if (Path.IsPathRooted(raw))
                return raw;

            return Path.Combine(environment.ContentRootPath, raw);
        }
    }
}
