using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitSubscriberWorkerService.Domain.Objects;

namespace RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost
{
    /// <summary>
    /// RabbitMqChannel class
    /// </summary>
    public class RabbitMQChannel : IRabbitMQChannel
    {
        private readonly IRabbitMQConnection _rabbitMqConnection;
        private readonly int _waitBetweenChannelRetries;
        private readonly int _channelRetryCount;
        private IModel _channel;
        private static readonly object ChannelLock = new object();
        private readonly ILogger<RabbitMQChannel> _logger;
        private readonly AppSettingsConfig _appsettingsConfig;




        /// <summary>
        ///  RabbitMQChannel constructor
        /// </summary>
        /// <param name="rabbitMqConnection"><see cref="IRabbitMQConnection"/></param>
        /// <param name="logger"><see ILogger cref="RabbitMQChannel"/></param>
        /// <param name="optionsMonitor"><see IOptionsMonitor cref="AppSettingsConfig"/></param>
        public RabbitMQChannel(IRabbitMQConnection rabbitMqConnection, ILogger<RabbitMQChannel> logger, IOptionsMonitor<AppSettingsConfig> optionsMonitor)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _appsettingsConfig = optionsMonitor.CurrentValue;
            _channelRetryCount = _appsettingsConfig.RetrievalSystemDataIntakeRabbitMQChannelRetryCount;
            _waitBetweenChannelRetries = _appsettingsConfig.RetrievalSystemDataIntakeRabbitMQWaitBetweenChannelRetries;
            _logger = logger;
        }


        /// <summary>
        /// Get Channel method
        /// </summary>
        /// <returns><see cref="IModel"/></returns>
        public IModel GetChannel()
        {
            lock (ChannelLock)
            {
                if (_channel == null)
                    TryCreateChannel();

                if (_channel.IsOpen && _rabbitMqConnection.Connection.IsOpen)
                    return _channel;

                //there is a problem with the channel/connection at this point so clean up the channel and try to create a new one
                Dispose();
                TryCreateChannel();

                return _channel;
            }
        }

        /// <summary>
        /// TryCreateChannel
        /// </summary>
        private void TryCreateChannel()
        {
            //attempt multiple times in case the connection is temporarily gone (should come back via auto-recover). if not blow up because that is all we can do at this point.
            var retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetry(_channelRetryCount, retryAttempt => TimeSpan.FromMilliseconds(_waitBetweenChannelRetries),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogError($"Unable to create RabbitMQ channel. Retry count: {retryCount}", exception);
                    });
            retryPolicy.Execute(() =>
            {
                if (!_rabbitMqConnection.Connection.IsOpen)
                    throw new InvalidOperationException($"RabbitMQ connection is not open while attempting to create the channel");
                _channel = _rabbitMqConnection.Connection.CreateModel();
                _channel.ModelShutdown += OnModelShutdown;
            });
        }

        /// <summary>
        /// OnModelShutdown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning($"RabbitMQ model/channel shutdown. Args: {e}");
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            if (_channel == null)
                return;

            _channel.Close();
            _channel.Dispose();
            _channel = null;
        }
    }
}