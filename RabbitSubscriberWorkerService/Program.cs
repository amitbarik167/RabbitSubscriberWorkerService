using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitSubscriberWorkerService.Domain.Objects;
using RabbitSubscriberWorkerService.Impl;
using RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost;
using RabbitSubscriberWorkerService.SubscriberManagement;
using RetrievalSystem.DataIntake.Service.Service.Subscribers;
using System.Reflection;


namespace RabbitSubscriberWorkerService
{
    /// <summary>
    /// Entry point
    /// </summary>
    public class Program
    {
        public static IConfiguration? Configuration { get; set; }
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// CreateHostBuilder
        /// </summary>
        /// <param name="args"></param>
        /// <returns><see cref="IHostBuilder"/></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((hostContext, config) =>
              {
                  var env = hostContext.HostingEnvironment;
                  config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                  if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                  {
                      var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                      if (appAssembly != null)
                      {
                          config.AddUserSecrets(appAssembly, optional: true);
                      }
                  }
                  config.AddEnvironmentVariables();
                  if (args != null)
                  {
                      config.AddCommandLine(args);
                  }
                  Configuration = config.Build();

              }).ConfigureServices((hostContext, services) =>
              {
                  services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
                  services.AddHostedService<Worker>();
                  services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
                  services.AddSingleton<IRabbitMQChannel, RabbitMQChannel>();

                  var testQueueName = Configuration?.GetSection("AppSettings")["TestQueueName"];
                  short deadLetterRetryCount = Convert.ToInt16(Configuration.GetSection("AppSettings")["RabbitMQDeadLetterRetryCount"]);
                  services.AddSingleton<IRabbitSubscriber, TestSubscriber>(x => new TestSubscriber(x.GetRequiredService<IRabbitMQConnection>(), testQueueName, deadLetterRetryCount, x.GetRequiredService<ILogger<TestSubscriber>>()));


                  var rabbitMQDelayedExchangeName = Configuration?.GetSection("AppSettings")["RabbitMQDelayedExchange"];

                
                  string ttlExpiration = Configuration.GetSection("AppSettings")["RetrievalSystemDataIntakeRabbitMQTTLExpiration"];
                  short maxReprocessingRetries = Convert.ToInt16(Configuration.GetSection("AppSettings")["RetrievalSystemDataIntakeRabbitMQReprocessingMaxRetries"]);
                  short xDelayInMilliseconds = Convert.ToInt16(Configuration.GetSection("AppSettings")["RetrievalSystemDataIntakeRabbitMQXDelayInMilliSeconds"]);

                  services.AddTransient<PublishMessage, PublishMessage>(x => new PublishMessage(x.GetRequiredService<IRabbitMQChannel>(), rabbitMQDelayedExchangeName, ttlExpiration, maxReprocessingRetries, deadLetterRetryCount, xDelayInMilliseconds, x.GetRequiredService<ILogger<PublishMessage>>()));

                  services.Configure<AppSettingsConfig>(Configuration?.GetSection("AppSettings"));
              }).UseWindowsService();
        }
    }
}
