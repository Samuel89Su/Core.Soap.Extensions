using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace SoapProxy.WebApiHost
{
    public class ApiBrokerMiddleware : OwinMiddleware
    {
        private static readonly IEnumerable<Type> _clientTypes = new List<Type>();

        static ApiBrokerMiddleware()
        {
            _clientTypes = (_clientTypes == null || _clientTypes.Count() == 0)
                ? Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ICommunicationObject).IsAssignableFrom(t) || typeof(SoapHttpClientProtocol).IsAssignableFrom(t))
                : _clientTypes;
        }

        public ApiBrokerMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            try
            {
                if ((PathServiceMap.Maps?.Count() ?? 0) == 0)
                {
                    await this.Next.Invoke(context);
                    return;
                }
                var pathMap = PathServiceMap.Maps.FirstOrDefault(m => request.Path.StartsWithSegments(new PathString(m.PathRoot)));
                if (pathMap == null)
                {
                    await this.Next.Invoke(context);
                    return;
                }
                var clientType = _clientTypes.FirstOrDefault(t => t.FullName.Equals(pathMap.ServiceClient));
                if (clientType == null)
                {
                    await this.Next.Invoke(context);
                    return;
                }

                var actionName = pathMap.PathActions.FirstOrDefault(m => request.Path.StartsWithSegments(new PathString(m.Key))).Value;
                MethodInfo actionMethod = null;
                if (string.IsNullOrWhiteSpace(actionName))
                {
                    actionMethod = clientType.GetMethod(request.Path.Value.Split('/').Last());
                }
                else
                {
                    actionMethod = clientType.GetMethod(actionName);
                }
                if (actionMethod == null)
                {
                    await this.Next.Invoke(context);
                    return;
                }

                var parameters = new object[0];
                var parameterInfos = actionMethod.GetParameters();
                if (parameterInfos != null || parameterInfos.Count() > 0)
                {
                    if (request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                    {
                        var form = await request.ReadFormAsync();
                        parameters = ParseForm(form, parameterInfos);
                    }
                    else if (request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var reader = new StreamReader(request.Body))
                        {
                            var raw = await reader.ReadToEndAsync();
                            parameters = new object[] { JsonConvert.DeserializeObject(raw, parameterInfos.First().ParameterType) };
                        }
                    }
                }

                var client = Activator.CreateInstance(clientType);
                var result = actionMethod.Invoke(client, parameters);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private object[] ParseForm(IFormCollection form, ParameterInfo[] parameterInfos)
        {
            return new object[0];
        }
    }
}
