using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoapProxy.WebApiHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var ipPort = ConfigurationManager.AppSettings.GetValues("IPPort")?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ipPort))
            {
                ipPort = "http://localhost:9000/";
            }

            //string baseAddress = "http://+:9000/"; //绑定所有地址，外网可以用ip访问 需管理员权限
            // 启动 OWIN host 
            WebApp.Start<Startup>(url: ipPort);
            Console.WriteLine("程序已启动,按任意键退出");
            Console.ReadLine();
        }
    }
}
