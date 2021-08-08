using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace SoapProxy.WebApiHost
{
    public class ApiBrokerMiddleware : OwinMiddleware
    {
        private static readonly List<Type> _clientTypes = new List<Type>();

        static ApiBrokerMiddleware()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                _clientTypes.AddRange(assembly.GetTypes().Where(t => typeof(SoapHttpClientProtocol).IsAssignableFrom(t)));
            }
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

                var client = CreateSvcClient(clientType, pathMap);
                var result = actionMethod.Invoke(client, parameters);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private object CreateSvcClient(Type svcType, PathServiceMap pathServiceMap)
        {
            var client = Activator.CreateInstance(svcType) as SoapHttpClientProtocol;

            client.Url = pathServiceMap.OriginSvcUrl;
            if (!string.IsNullOrWhiteSpace(pathServiceMap.Http_Proxy))
            {
                client.Proxy = new WebProxy(pathServiceMap.Http_Proxy);
            }

            return client;
        }

        private object[] ParseForm(IFormCollection form, ParameterInfo[] parameterInfos)
        {
            return new object[0];
        }
    }
}
