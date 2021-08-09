using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SoapJsonConversionMiddleware
{
    public class OperationDescription
    {
        public ContractDescription Contract { get; private set; }
        public string SoapAction { get; private set; }
        public string ReplyAction { get; private set; }
        public string Name { get; private set; }
        public MethodInfo DispatchMethod { get; private set; }
        public bool IsOneWay { get; private set; }

        public OperationDescription(ContractDescription contract, MethodInfo operationMethod, OperationContractAttribute contractAttribute)
        {
            Contract = contract;
            Name = contractAttribute.Name ?? operationMethod.Name;
            SoapAction = string.IsNullOrWhiteSpace(contractAttribute.Action)
                ? $"{contract.Namespace.Trim('/')}/{contract.Name}/{Name}"
                : contractAttribute.Action.StartsWith(contract.Namespace, StringComparison.OrdinalIgnoreCase)
                ? contractAttribute.Action
                : $"{contract.Namespace.Trim('/')}/{contractAttribute.Action.Trim('/')}";
            IsOneWay = contractAttribute.IsOneWay;
            ReplyAction = contractAttribute.ReplyAction;
            DispatchMethod = operationMethod;
        }
    }
}
