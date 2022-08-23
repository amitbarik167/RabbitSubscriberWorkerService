using System;
using System.Collections.Generic;
using System.Linq;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitSubscriberWorkerService.Domain.Objects;

namespace RabbitSubscriberWorkerService.Messaging.RabbitMQ.vHost.TestHost
{
    /// <summary>
    /// RabbitMQConnection Connection Class
    /// </summary>
    //this class is registered as a singleton.
    //https://www.rabbitmq.com/dotnet-api-guide.html
    public class RabbitMQConnection : IRabbitMQConnection
    {

        private static readonly object ConnectionLock = new object();
        private readonly List<string> _rabbitHosts;
        private readonly short _connectionAttemptCount;
        private readonly short _waitBetweenConnectionRetries;
        private readonly IConnectionFactory _connectionFactory;
        private const int ConnectionCloseTimeout = 10000;
        private IConnection _connection;
        private readonly ILogger<RabbitMQConnection> _logger;
        private readonly short _rabbitNetworkRecoveryInterval;
        private readonly short _rabbitMqPort;
        private readonly string _rabbitMqVHost;
        private readonly string _rabbitMqUsername;
        private readonly string _rabbitMqPassword;
        private readonly AppSettingsConfig _appsettingsConfig;

        /// <summary>
        /// RabbitMQConnection constructor
        /// </summary>
        /// <param name="connectionFactory"></param>
        /// <param name="rabbitHosts"></param>
        /// <param name="connectionAttemptCount"></param>
        /// <param name="waitBetweenConnectionRetries"></param>
        public RabbitMQConnection(
           IOptionsMonitor<AppSettingsConfig> optionsMonitor, ILogger<RabbitMQConnection> logger
            )
        {
            _logger = logger;
            _appsettingsConfig = optionsMonitor.CurrentValue;
            _connectionAttemptCount = _appsettingsConfig.RabbitMQConnectionAttemptCount;
            _waitBetweenConnectionRetries = _appsettingsConfig.RabbitMQWaitBetweenConnectionRetries;
            _rabbitNetworkRecoveryInterval = _appsettingsConfig.RabbitMQNetworkRecoveryInterval;
            _rabbitMqPort = _appsettingsConfig.RabbitMQPort;
            _rabbitMqVHost = _appsettingsConfig.RabbitMQVirtualHost;
            _rabbitMqPassword = _appsettingsConfig.RabbitMQEncryptedPassword;
            _rabbitMqUsername = _appsettingsConfig.RabbitMQUserName;

            _connectionFactory = new ConnectionFactory
            {
                NetworkRecoveryInterval = TimeSpan.FromMilliseconds(_rabbitNetworkRecoveryInterval),
                Port = _rabbitMqPort,
                VirtualHost = _rabbitMqVHost,
                UserName = _rabbitMqUsername,
                //Password = Cryptography.Decrypt(_rabbitMqPassword)
            };
            _rabbitHosts = _appsettingsConfig.RabbitMQHosts.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(s => s.Trim()).ToList();

        }

        /// <summary>
        /// Connection
        /// </summary>
        public IConnection Connection => GetConnection();

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            lock (ConnectionLock)
            {
                if (_connection == null)
                    return;

                _connection.Close(TimeSpan.FromMilliseconds(ConnectionCloseTimeout));
                _connection.Dispose();
                _connection = null;
            }
        }


        /// <summary>
        /// Get Connection
        /// </summary>
        /// <returns></returns>
        private IConnection GetConnection()
        {
            lock (ConnectionLock)
            {
                if (_connection == null)
                    EstablishConnection(); //only call this once, after that auto-recovery will take over if there is a problem with the connection
                return _connection;
            }
        }

        /// <summary>
        /// Establish Connection
        /// </summary>
        private void EstablishConnection()
        {

            var retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetry(_connectionAttemptCount,
                    retryAttempt => TimeSpan.FromMilliseconds(_waitBetweenConnectionRetries),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogError($"Unable to initialize RabbitMQ connection. Retry count: {retryCount}", exception);
                    });
            retryPolicy.Execute(() =>
            {
                _connection = _connectionFactory.CreateConnection(_rabbitHosts);
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.ConnectionBlocked += OnConnectionBlocked;
                _connection.ConnectionUnblocked += OnConnectionUnblocked;
                _connection.CallbackException += ConnectionOnCallbackException;
            });
        }

        /// <summary>
        /// ConnectionOnCallbackException
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionOnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError($"RabbitMQ connection callback exception: {e.Detail}.", e.Exception);
        }

        /// <summary>
        /// OnConnectionUnblocked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionUnblocked(object sender, EventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection is unblocked.");
        }

        /// <summary>
        /// OnConnectionBlocked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning($"RabbitMQ connection is blocked. Reason: {e.Reason}");
        }

        /// <summary>
        /// OnConnectionShutdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning($"RabbitMQ connection is shutting down. Args: {e}");
        }
    }
}
