using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
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
                    if (request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var reader = new StreamReader(request.Body))
                        {
                            var raw = await reader.ReadToEndAsync();
                            parameters = new object[] { JsonConvert.DeserializeObject(raw, parameterInfos.First().ParameterType) };
                        }
                    }
                    else
                    {
                        IEnumerable<KeyValuePair<string, string[]>> query = null;
                        if (request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                        {
                            query = await request.ReadFormAsync();
                        }
                        if (request.QueryString.HasValue)
                        {
                            query = query.Concat(request.Query);
                        }
                        parameters = ParseForm(query, parameterInfos);
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

        private object[] ParseForm(IEnumerable<KeyValuePair<string, string[]>> form, ParameterInfo[] parameterInfos)
        {
            if (form == null || form.Count()==0)
            {
                return new object[] { };
            }
            var keyGroups = form.GroupBy(kv => kv.Key);
            var formDic = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in keyGroups)
            {
                formDic[group.Key] = group.SelectMany(g => g.Value);
            }

            var arguments = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var parameter = parameterInfos[i];
                if (!formDic.TryGetValue(parameter.Name, out IEnumerable<string> values) || values == null || values.Count() == 0)
                {
                    arguments[i] = null;
                }
                else if (typeof(string) == parameter.ParameterType)
                {
                    arguments[i] = values.First();
                }
                else if (parameter.ParameterType == typeof(Guid) || parameter.ParameterType.IsPrimitive)
                {
                    arguments[i] = JToken.FromObject(values.First()).ToObject(parameter.ParameterType);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(parameter.ParameterType))
                {
                    arguments[i] = JToken.FromObject(values).ToObject(parameter.ParameterType);
                }
            }

            return new object[0];
        }
    }
}
