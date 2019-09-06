using System;
using System.Reflection;
using Application.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTracing;
using OpenTracing.Util;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Prometheus;

namespace Application
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            CreateLogger();
        }

        public void CreateLogger()
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Debug()
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(Environment.GetEnvironmentVariable("ELASTICSEARCH_URI")))
                    {
                        MinimumLogEventLevel = LogEventLevel.Verbose,
                        AutoRegisterTemplate = true
                    }).CreateLogger();
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Set compability mode for mvc
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
            services.AddSingleton<ITracer>(serviceProvider =>
            {
                string serviceName = Assembly.GetEntryAssembly().GetName().Name;

                Environment.SetEnvironmentVariable("JAEGER_SERVICE_NAME", serviceName);

                var loggerFactory = new LoggerFactory();
                try
                {
                    // add agenthost and port
                    var config = Jaeger.Configuration.FromEnv(loggerFactory);
                    var tracer = config.GetTracer();

                    GlobalTracer.Register(tracer);
                    return tracer;
                }
                catch (System.Exception)
                {
                    System.Console.WriteLine("Couldn't register logger");
                }
                return null;

            });

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            services.AddOpenTracing();

            APIDocumentationInitializer.ApiDocumentationInitializer(services);
            StartupDatabaseInitializer.InitializeDatabase(services);

            services.AddHealthChecks();

            CorsConfig.AddCorsPolicy(services);
        }

        public void ConfigureDevelopmentSevices(IServiceCollection services)
        {
            Environment.SetEnvironmentVariable("JAEGER_AGENT_HOST", "localhost");
            Environment.SetEnvironmentVariable("JAEGER_AGENT_PORT", "6831");
            Environment.SetEnvironmentVariable("JAEGER_SAMPLER_TYPE", "const");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseHealthChecks("/api/health");
            app.UseMetricServer();
            app.UseRequestMiddleware();

            StartupDatabaseInitializer.MigrateDatabase(app);
            APIDocumentationInitializer.AllowAPIDocumentation(app);
            CorsConfig.AddCors(app);

            app.UseMvc();
        }
    }
}