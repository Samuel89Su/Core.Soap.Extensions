using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace SoapJsonConversion.Middleware
{
    public class ServiceDescription
    {
        public Type ServiceType { get; private set; }
        public ContractDescription Contract { get; private set; }
        public IEnumerable<OperationDescription> Operations => Contract.Operations;

        public ServiceDescription(Type serviceType)
        {
            ServiceType = serviceType;

            var serviceContractAttr = serviceType.GetCustomAttributes<ServiceContractAttribute>().FirstOrDefault()
                ?? throw new ArgumentException($"Type {serviceType} do NOT has Attribute {typeof(ServiceContractAttribute)}!");

            Contract = new ContractDescription(this, ServiceType, serviceContractAttr);
        }
    }
}
