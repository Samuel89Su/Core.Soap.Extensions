using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SoapProxy.WebApiHost
{
    public class ValuesController : ApiController
    {
        public async Task<string> Get()
        {
            TestWebService.AccountServiceSoapClient;

            return "value";
        }
    }
}
