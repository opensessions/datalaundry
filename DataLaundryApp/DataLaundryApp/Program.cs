using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataLaundryApp
{
    public class Program
    {
        public static void Main(string[] args)
        {           
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        //public static void Main(string[] args)
        // {
        //     CreateWebHostBuilder(args).Build().Run();
        //     // var host = new WebHostBuilder()
        //     // .UseKestrel()
        //     // .UseContentRoot(Directory.GetCurrentDirectory())
        //     // .UseIISIntegration()
        //     // .UseStartup<Startup>()
        //     // .Build();

        //     //  host.Run();
        // }

        // public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //     WebHost.CreateDefaultBuilder(args)
        //         .UseKestrel(options=>{options.AddServerHeader=false;})
        //         .UseContentRoot(Directory.GetCurrentDirectory())
        //         .UseIISIntegration()
        //         .UseStartup<Startup>();
    
    }
}
