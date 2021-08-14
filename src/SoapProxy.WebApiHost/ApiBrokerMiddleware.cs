using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections;
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

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

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
                    logger.Debug($"No Service Map Configured!");
                    await this.Next.Invoke(context);
                    return;
                }
                var pathMap = PathServiceMap.Maps.FirstOrDefault(m => request.Path.StartsWithSegments(new PathString(m.PathRoot)));
                if (pathMap == null)
                {
                    logger.Debug($"No Service Map for Path:{request.Path}!");
                    await this.Next.Invoke(context);
                    return;
                }
                var clientType = _clientTypes.FirstOrDefault(t => t.FullName.Equals(pathMap.ServiceClient));
                if (clientType == null)
                {
                    logger.Warn($"ClientProxy:{pathMap.ServiceClient} not found!");
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
                    logger.Warn($"Action:{actionName} not found in ClientProxy:{pathMap.ServiceClient}!");
                    await this.Next.Invoke(context);
                    return;
                }

                var parameters = new object[0];
                var parameterInfos = actionMethod.GetParameters();
                if (parameterInfos != null || parameterInfos.Count() > 0)
                {
                    if (request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var reader = new StreamReader(request.Body))
                        {
                            var raw = await reader.ReadToEndAsync();
                            parameters = ParseJTokenToParameters(JsonConvert.DeserializeObject<JToken>(raw), parameterInfos);
                        }
                    }
                    else
                    {
                        logger.Warn($"only support Content-Type: application/json!");
                        throw new ArgumentException($"only support Content-Type: application/json");
                    }
                }

                var client = CreateSvcClient(clientType, pathMap);

                logger.Info($"Invoke ClientProxy:{pathMap.ServiceClient}.{actionMethod.Name}"
                    + (parameters != null && parameters.Length > 0
                    ? $" with parameters:{JsonConvert.SerializeObject(parameters)}"
                    : string.Empty));

                var result = actionMethod.Invoke(client, parameters);
                logger.Info($"Invoke ClientProxy:{pathMap.ServiceClient}.{actionMethod.Name} done"
                    +
                    (result != null
                    ? $" with return{JsonConvert.SerializeObject(result)}"
                    : string.Empty
                    ));

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    logger.Error(ex.InnerException, ex.InnerException.Message);
                }
                else
                {
                    logger.Error(ex, ex.Message);
                }
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

        private object[] ParseJTokenToParameters(JToken jtoken, ParameterInfo[] parameterInfos)
        {
            if (jtoken == null)
            {
                return new object[] { };
            }
            if (parameterInfos.Length == 1)
            {
                return new object[] { jtoken.ToObject(parameterInfos.First().ParameterType) };
            }

            if (jtoken.Children().Count() == 0)
            {
                return new object[] { };
            }

            var properties = jtoken.Children().Cast<JProperty>();
            if (properties == null || properties.Count() == 0)
            {
                return new object[] { };
            }

            var arguments = new object[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var parameter = parameterInfos[i];
                var argument = properties.FirstOrDefault(p => p.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase));
                if (argument == null)
                {
                    arguments[i] = null;
                }
                else
                {
                    arguments[i] = argument.ToObject(parameter.ParameterType);
                }
            }

            return new object[0];
        }
    }
}
