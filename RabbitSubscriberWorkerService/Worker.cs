using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitSubscriberWorkerService.Domain.Objects;
using RabbitSubscriberWorkerService.Events;
using RabbitSubscriberWorkerService.SubscriberManagement;

namespace RabbitSubscriberWorkerService
{
    /// <summary>
    /// Worker class 
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Exception _asyncException;
        internal static string? serviceName;
        public event ServiceStateChangedEventHandler ServiceStateChanged;
        private readonly AppSettingsConfig _appsettingsConfig;
        private readonly ISubscriptionManager _subscriptionManager;
        public Worker(ILogger<Worker> logger, IOptionsMonitor<AppSettingsConfig> optionsMonitor, ISubscriptionManager subscriptionManager)
        {
            _logger = logger;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            _appsettingsConfig = optionsMonitor.CurrentValue;
            serviceName = _appsettingsConfig.ApplicationName;
            _subscriptionManager = subscriptionManager;

        }

        /// <summary>
        /// StartAsync
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>Task</returns>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service : {0} started", serviceName);
            try
            {
                if (_asyncException != null)
                {
                    _logger.LogError("An exception occurred during Service instantiation.  {0} cannot start.", _asyncException, serviceName);
                    _asyncException = null;
                    StopAsync(cancellationToken).Wait(cancellationToken);
                    return null;
                }
                _subscriptionManager.InitializeSubscriptions();
                ServiceStateChanged?.Invoke(new ServiceStateChangedEventArgs(ServiceState.Started));
                _logger.LogInformation("{0} has been started.", serviceName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
            return base.StartAsync(cancellationToken);
        }



        /// <summary>
        /// ExecuteAsync
        /// </summary>
        /// <param name="stoppingToken"><see cref="CancellationToken"></see></param>
        /// <returns>Task</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        /// <summary>
        /// Stop the process
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>Task</returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("RabbitMQ connection is closed.");
        }

        /// <summary>
        /// Store the exception and begin the shutdown process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"><see cref="UnhandledExceptionEventArgs"/></param>
        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            _asyncException = e.ExceptionObject as Exception;
            _logger.LogInformation("Exception:", _asyncException.Message);
            var cancelSource = new CancellationTokenSource();
            base.StopAsync(cancelSource.Token);
        }
    }
}
