using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Microsoft.Owin.Hosting.WebApp.Start<Startup>("http://127.0.0.1:9009"))
            {
                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine("[{0}]-->{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), "启动文档生成器服务");
                Console.WriteLine("[{0}]-->{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), "准备生成文档信息");
                Generator generator = Generator.GetGenerator();
                generator.GeneratorApi();
                Console.WriteLine("[{0}]-->{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), "生成文档信息成功");
                Console.ReadLine();
            }
        }
    }
}
