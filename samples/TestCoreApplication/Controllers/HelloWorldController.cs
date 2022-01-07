using Microsoft.AspNetCore.Mvc;
using SoapJsonConversion.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace TestCoreApplication.Controllers
{
    [ServiceContract(Namespace = "http://webservice.com")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        [OperationContract(Action = "HelloWorld")]
        [HttpPost()]
        public string HelloWorld()
        {
            return "Hello World";
        }
    }
}
