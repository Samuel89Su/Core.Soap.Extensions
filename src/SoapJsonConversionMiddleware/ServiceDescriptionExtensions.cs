using System;
using System.Collections.Generic;
using System.Linq;

namespace SoapJsonConversion.Middleware
{
    public class ServiceDescriptionExtensions
    {
        public readonly static Dictionary<string, HashSet<Type>> PathControllers = new Dictionary<string, HashSet<Type>>(StringComparer.OrdinalIgnoreCase);
        public readonly static Dictionary<Type, ServiceDescription> ServiceDescriptions = new Dictionary<Type, ServiceDescription>();

        public static bool TryGetMatchControllers(string path, out IEnumerable<Type> controllers)
        {
            controllers = new List<Type>();
            if (!string.IsNullOrWhiteSpace(path) && PathControllers.TryGetValue(path, out HashSet<Type> types) && types != null)
            {
                controllers = types.ToList();
                return true;
            }
            return false;
        }

        public static bool TryGetMatchActions(string path, string soapAction, out Type controllerType, out OperationDescription operationDescription)
        {
            controllerType = null;
            operationDescription = null;
            if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(soapAction)
                && PathControllers.TryGetValue(path, out HashSet<Type> controllers) && controllers != null
                && controllers != null)
            {
                foreach (var controller in controllers)
                {
                    if (ServiceDescriptions.TryGetValue(controller, out ServiceDescription serviceDescription) && serviceDescription != null
                        && serviceDescription.Operations != null)
                    {
                        controllerType = controller;
                        operationDescription = serviceDescription.Operations.FirstOrDefault(o => o.FullSoapAction.Equals(soapAction, StringComparison.OrdinalIgnoreCase));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
