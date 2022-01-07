using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace SoapJsonConversion.Middleware
{
    public static class SOAPMiddlewareExtensions
    {
        private static object _lock = new object();
        private static bool _initialized = false;

        public static IApplicationBuilder UseSOAPMiddleware<T>(this IApplicationBuilder builder, [NotNull] string path) where T : ControllerBase
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (!_initialized)
                {
                    lock (_lock)
                    {
                        if (!_initialized)
                        {
                            builder.UseMiddleware<SOAPMiddleware>();
                            _initialized = true;
                        }
                    }
                }

                lock (_lock)
                {
                    if (!ServiceDescriptionExtensions.PathControllers.TryGetValue(path, out HashSet<Type> controllers) || controllers == null)
                    {
                        ServiceDescriptionExtensions.PathControllers[path] = controllers = new HashSet<Type>();
                    }
                    var controllerType = typeof(T);
                    if (!controllers.Contains(controllerType))
                    {
                        try
                        {
                            ServiceDescriptionExtensions.ServiceDescriptions[controllerType] = new ServiceDescription(controllerType);

                            controllers.Add(typeof(T));
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }

            return builder;
        }
    }
}
