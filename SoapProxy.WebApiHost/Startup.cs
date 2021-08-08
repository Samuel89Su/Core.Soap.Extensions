using Microsoft.Owin;
using Owin;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

[assembly: OwinStartup(typeof(SoapProxy.WebApiHost.Startup))]

namespace SoapProxy.WebApiHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // load assembly
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var unloadDlls = Directory.GetFiles(baseDir, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(d =>
                    !assemblies.Any(a =>
                        a.GetName().Name
                        .Equals(d.Replace(baseDir, string.Empty)
                        .Replace(".dll", string.Empty), StringComparison.OrdinalIgnoreCase)
                    )
                ).ToArray();

            foreach (var dllPath in unloadDlls)
            {
                var assembly = Assembly.LoadFile(dllPath);
                AppDomain.CurrentDomain.Load(assembly.GetName());

                var webSvcClients = assembly.GetTypes().Where(t => typeof(System.Web.Services.Protocols.SoapHttpClientProtocol).IsAssignableFrom(t));
                if ((webSvcClients?.Count() ?? 0) > 0)
                {
                    Console.WriteLine($"Load WebServiceClient {string.Join(",", webSvcClients.Select(t => t.Name))} from {assembly.FullName}");
                }
            }

            appBuilder.Use<ApiBrokerMiddleware>();

            appBuilder.UseWebApi(config);
        }
    }
}
