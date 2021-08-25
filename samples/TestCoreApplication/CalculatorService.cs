using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace TestCoreApplication
{
    [ServiceContract]
    public class CalculatorService //: ICalculatorService
    {
        [OperationContract]
        public double Add(double x, double y) => x + y;
        [OperationContract]
        public double Divide(double x, double y) => x / y;
        [OperationContract]
        public double Multiply(double x, double y) => x * y;
        [OperationContract]
        public double Subtract(double x, double y) => x - y;
        [OperationContract]
        public string Get(string str) => $"{str} Hello World!";
    }

    //[ServiceContract]
    //public interface ICalculatorService
    //{
    //    [OperationContract]
    //    double Add(double x, double y);
    //    [OperationContract]
    //    double Subtract(double x, double y);
    //    [OperationContract]
    //    double Multiply(double x, double y);
    //    [OperationContract]
    //    double Divide(double x, double y);

    //    [OperationContract]
    //    string Get(string str);
    //}
}
