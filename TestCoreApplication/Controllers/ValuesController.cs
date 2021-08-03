using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestCoreApplication.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public async Task<string> Get()
        {
            return "value";
        }
    }
}
