using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public class StartupDevelopment
    {
        public StartupDevelopment(IConfiguration configuration)
        {

            Environment.SetEnvironmentVariable("ELASTICSEARCH_URI", "http://localhost:9200");
            _startup = new Startup(configuration);
        }

        private Startup _startup;
        public void ConfigureServices(IServiceCollection services)
        {
            _startup.ConfigureServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            _startup.Configure(app, env);
        }
    }
}